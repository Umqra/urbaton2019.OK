using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.WebSockets;
using System.Threading.Tasks;
using CleanCityCore;
using CleanCityCore.EmailSender;
using CleanCityCore.Model;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;

namespace CleanCityBot
{
    public class BotUserInteraction
    {
        private readonly IBotManager manager;
        private readonly ICleanCityApi cleanCityApi;
        private bool isStarted;

        public BotUserInteraction(IBotManager manager, ICleanCityApi cleanCityApi)
        {
            this.manager = manager;
            this.cleanCityApi = cleanCityApi;
            isStarted = false;
        }

        public void RunIfNeeded()
        {
            if (!isStarted)
            {
                isStarted = true;
                Task.Factory.StartNew(async () =>
                {
                    try
                    {
                        await Process();
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e);
                        throw;
                    }
                    finally
                    {
                        isStarted = false;
                    }
                });
            }
        }

        private async Task Process()
        {
            var firstResponse = await manager.GetResponseAsync();
            if (firstResponse.Text == "/start" || firstResponse.Text == "/help")
            {
                await ProcessHelp();
            }

            else if (firstResponse.Text == "/register")
            {
                await ProcessRegistration();
            }

            else if (firstResponse.Text == "/report")
            {
                await ProcessReport();
            }
        }

        private async Task ProcessHelp()
        {
            await manager.SendTextMessageAsync(
                "Бот общественный квартальный.\n" +
                "Сперва вам нужно пройти процесс регистрации с помощью команды /register\n" +
                "После этого вы сможете создавать обращения квартальным города Екатеринбург с помощью команды /report",
                new ReplyKeyboardRemove());
        }

        private async Task ProcessRegistration()
        {
            var userName = string.Empty;
            while (string.IsNullOrWhiteSpace(userName))
            {
                await manager.SendTextMessageAsync("Введите ваши имя и фамилию:");
                userName = (await manager.GetResponseAsync()).Text;
            }

            var address = string.Empty;
            while (string.IsNullOrWhiteSpace(address))
            {
                await manager.SendTextMessageAsync("Введите ваш адрес:");
                address = (await manager.GetResponseAsync()).Text;
            }

            var email = string.Empty;
            while (string.IsNullOrWhiteSpace(email))
            {
                await manager.SendTextMessageAsync("Введите email, по которому можно будет с вами связаться:");
                email = (await manager.GetResponseAsync()).Text;
            }
        }

        private async Task ProcessReport()
        {
            var attachments = new List<Attachment>();
            var markup = new ReplyKeyboardMarkup(new[]
            {
                new[] {new KeyboardButton("Грязь")},
                new[] {new KeyboardButton("Парковка")},
                new[] {new KeyboardButton("Ещё что-то")}
            });
            var makeReport = new ReplyKeyboardMarkup(new[]
            {
                new[] {new KeyboardButton("Сформировать обращение")},
            });
            var resetMarkup = new ReplyKeyboardRemove();

            var subject = string.Empty;
            while (string.IsNullOrWhiteSpace(subject))
            {
                await manager.SendTextMessageAsync("Введите тему обращения:", markup);
                subject = (await GetResponse(attachments)).Text;
            }

            var reportText = string.Empty;
            while (string.IsNullOrWhiteSpace(reportText))
            {
                await manager.SendTextMessageAsync("Подробно опишите детали проблемы:", resetMarkup);
                reportText = (await GetResponse(attachments)).Text;
            }

            GeoLocation location = null;
            while (location == null)
            {
                await manager.SendTextMessageAsync(
                    "Укажите своё местоположение",
                    new ReplyKeyboardMarkup(
                        KeyboardButton.WithRequestLocation("Отправить моё текущее местоположение")));
                var locationMessage = await GetResponse(attachments);
                if (locationMessage.Location != null)
                {
                    location = new GeoLocation
                    {
                        Latitude = locationMessage.Location.Latitude,
                        Longitude = locationMessage.Location.Longitude,
                    };
                }
            }

            while (true)
            {
                var (count, caption) =
                    Pluralizator.Pluralize(attachments.Count, "фотографий", "фотографию", "фотографии");
                await manager.SendTextMessageAsync(
                    $"Мы получили от Вас уже {count} {caption}. Вы можете отправить ещё фотографии или \"Сформировать обращение\"",
                    makeReport);
                var message = await GetResponse(attachments);
                if (message.Text != null && message.Text.Contains("обращение"))
                {
                    break;
                }
            }

            var initialReport = new InitialReport
            {
                Subject = subject,
                ReportText = reportText,
                Location = location,
                Attachments = attachments.ToArray(),
            };
            var reportId = cleanCityApi.SendReport(initialReport);
            var report = cleanCityApi.GetReport(reportId);
            var responsible = cleanCityApi.GetResponsible(report.ResponsibleId);
            await manager.SendTextMessageAsync(
                $"Обращение успешно сформировано, мы уведомим соответствующего квартального о проблеме:\n" +
                $"{responsible.Name}");
        }

        private async Task<Message> GetResponse(List<Attachment> attachments)
        {
            var message = await manager.GetResponseAsync();
            TryAddPhoto(attachments, message);
            return message;
        }

        private void TryAddPhoto(List<Attachment> attachments, Message message)
        {
            if (message.Photo == null || !message.Photo.Any())
            {
                return;
            }

            var highResolutionPhoto = message.Photo.OrderByDescending(x => x.Width * x.Height).First();
            Console.WriteLine($"Download file: {highResolutionPhoto.FileId}");
            var photo = DownloadFile(highResolutionPhoto.FileId);
            attachments.Add(new Attachment
            {
                Data = photo,
                Filename = "attachment.jpg",
            });
        }

        private byte[] DownloadFile(string fileId)
        {
            var photoData = new MemoryStream();
            manager.Bot.GetInfoAndDownloadFileAsync(fileId, photoData).GetAwaiter().GetResult();
            return photoData.ToArray();
        }
    }
}