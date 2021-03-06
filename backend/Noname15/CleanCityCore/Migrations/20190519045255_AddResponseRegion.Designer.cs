﻿// <auto-generated />
using System;
using CleanCityCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;
using NpgsqlTypes;

namespace CleanCityCore.Migrations
{
    [DbContext(typeof(CleanCityContext))]
    [Migration("20190519045255_AddResponseRegion")]
    partial class AddResponseRegion
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn)
                .HasAnnotation("ProductVersion", "2.2.3-servicing-35854")
                .HasAnnotation("Relational:MaxIdentifierLength", 63);

            modelBuilder.Entity("CleanCityCore.Sql.EmailMessageSql", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<byte[]>("Data");

                    b.Property<bool>("IsSent");

                    b.Property<Guid>("ResponsibleId");

                    b.HasKey("Id");

                    b.HasIndex("IsSent");

                    b.ToTable("Emails");
                });

            modelBuilder.Entity("CleanCityCore.Sql.ReportSql", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("Body");

                    b.Property<DateTime>("CreationDate");

                    b.Property<NpgsqlPoint>("Location");

                    b.Property<byte[]>("Payload");

                    b.Property<Guid>("ResponsibleId");

                    b.Property<string>("Subject");

                    b.Property<long>("UserId");

                    b.HasKey("Id");

                    b.ToTable("Reports");
                });

            modelBuilder.Entity("CleanCityCore.Sql.ResponsibleDoublerSql", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("Email");

                    b.Property<string>("Name");

                    b.Property<Guid>("OriginalId");

                    b.HasKey("Id");

                    b.HasIndex("OriginalId");

                    b.ToTable("ResponsibleDoublerSql");
                });

            modelBuilder.Entity("CleanCityCore.Sql.ResponsibleSql", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("Email");

                    b.Property<bool>("IsActive");

                    b.Property<string>("Name");

                    b.Property<string>("ResponseRegion");

                    b.HasKey("Id");

                    b.ToTable("ResponsibleSql");
                });

            modelBuilder.Entity("CleanCityCore.Sql.UserSql", b =>
                {
                    b.Property<long>("UserId")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("Address");

                    b.Property<string>("Email");

                    b.Property<string>("Username");

                    b.HasKey("UserId");

                    b.ToTable("Users");
                });

            modelBuilder.Entity("CleanCityCore.Sql.ResponsibleDoublerSql", b =>
                {
                    b.HasOne("CleanCityCore.Sql.ResponsibleSql", "OriginalResponsible")
                        .WithMany("DoublerList")
                        .HasForeignKey("OriginalId")
                        .HasConstraintName("ForeignKey_Doubler_Responsible")
                        .OnDelete(DeleteBehavior.Cascade);
                });
#pragma warning restore 612, 618
        }
    }
}
