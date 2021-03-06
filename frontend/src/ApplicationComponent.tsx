import * as React from "react";
import {useState, useEffect} from "react";
import {IBackendApi} from "./BackendApi";
import {
    Grid,
    Container,
    Segment,
    Menu,
    List,
    Button,
    Modal,
    Form,
    Header,
    Image,
    Icon,
    Pagination
} from 'semantic-ui-react'
import {IResponsible} from "./IResponsible";
import {withRouter} from "react-router";
import {HashRouter as Router, Route, Link, Switch} from "react-router-dom";
import {IReport} from "./IReport";


interface IApplicationComponentProps {
    backendApi: IBackendApi;
}

function ReportListComponent(props: IApplicationComponentProps) {
    const [reports, setReports] = useState([]);
    useEffect(() => {
        async function loadAsync() {
            const {backendApi} = props;
            const reports = await backendApi.getReports(0, 10);
            setReports(reports);
        }

        loadAsync();
    }, []);
    const {backendApi} = props;
    return (
        <List divided>
            {reports.map(r => <List.Item key={r.id} as={Link} to={`/report/${r.id}`}>
                <List.Icon name='content'/>
                <List.Content>
                    <List.Header>{r.subject}</List.Header>
                    <List.Description>{r.reportText}</List.Description>
                </List.Content>
            </List.Item>)}
        </List>
    );
}

interface IAddDoublerFormComponentProps {
    responsibleName: string;
    responsibleId: string;
    addDoubler: (responsible: IResponsible, responsibleId: string) => Promise<void>;
}

function AddDoublerFormComponent(props: IAddDoublerFormComponentProps) {
    const [name, setName] = useState("");
    const [email, setEmail] = useState("");
    const {responsibleId, responsibleName, addDoubler} = props;
    const [openModal, setOpenModal] = useState(false);
    return (
        <Modal
            open={openModal}
            onClose={() => setOpenModal(false)}
            trigger={<Button onClick={() => setOpenModal(true)}>Подписаться на сообщения</Button>}
        >
            <Segment>
                <Form>
                    <Header>Подписка на сообщения {responsibleName}</Header>
                    <Form.Field>
                        <label>Ваше имя</label>
                        <input placeholder='Имя Фамилия' value={name} onChange={e => setName(e.target.value)}/>
                    </Form.Field>
                    <Form.Field>
                        <label>Email</label>
                        <input placeholder='mail@domain.com' value={email} onChange={e => setEmail(e.target.value)}/>
                    </Form.Field>
                    <Button onClick={async () => {
                        setOpenModal(false);
                        await addDoubler({
                            email: email,
                            name: name,
                            id: null,
                            responseRegion: null
                        }, responsibleId)
                    }}>Подписаться на сообщения квартального</Button>
                </Form>
            </Segment>
        </Modal>

    );
}

function ResponsibleListComponent(props: IApplicationComponentProps) {
    const [responsibles, setResponsibles] = useState([]);
    const [page, setPage] = useState(1);
    useEffect(() => {
        async function loadAsync() {
            const {backendApi} = props;
            const responsibles = await backendApi.getResponsibles((page - 1) * 10, 10);
            if (responsibles.length == 0) {
                setPage(page - 1);
                return;
            }
            setResponsibles(responsibles);
        }

        loadAsync();
    }, [page]);
    const {backendApi} = props;
    return (
        <>
            <List divided>
                {responsibles.map((r, i) => {
                    return (<List.Item key={i}>
                        <List.Content floated='right'>

                            <AddDoublerFormComponent responsibleId={r.id}
                                                     responsibleName={r.name}
                                                     addDoubler={async (doubler, responsibleId) => {
                                                         await backendApi.addDoubler(responsibleId, doubler);
                                                     }}/>
                        </List.Content>
                        <List.Icon name='users'/>
                        <List.Content>
                            <List.Header>{r.name}</List.Header>
                            <List.Description>{r.email} ({r.responseRegion})</List.Description>
                        </List.Content>
                    </List.Item>);
                })}
            </List>
            <Pagination totalPages={100} activePage={page}
                        firstItem={null}
                        lastItem={null}
                        prevItem={{content: <Icon name='angle double left'/>, icon: true}}
                        nextItem={{content: <Icon name='angle double right'/>, icon: true}}
                        siblingRange={1}
                        boundaryRange={0}
                        onPageChange={(e, data) => {
                            if (page < data.activePage && responsibles.length < 10) {
                                return;
                            }
                            // @ts-ignore
                            setPage(data.activePage)
                        }}/>
        </>);
}

interface IMainPageApplicationComponentProps {
    backendApi: IBackendApi;
    reportId: string;
    activeItem: string;
}

export function MainPageApplicationComponent(props: IMainPageApplicationComponentProps) {
    const {reportId, activeItem} = props;
    return (
        <Segment>
            <Menu>
                <Menu.Item>
                    <img src='https://raw.githubusercontent.com/Umqra/urbaton2019.OK/master/img/logo.png'/>
                </Menu.Item>
                <Menu.Item
                    name='reports'
                    active={activeItem === 'reports'}
                    as={Link}
                    to={"/reports"}
                >
                    Список обращений
                </Menu.Item>
                <Menu.Item
                    name='responsibles'
                    active={activeItem === 'responsibles'}
                    as={Link}
                    to={"/responsibles"}
                >
                    Список квартальных города Екатеринбург
                </Menu.Item>
                {reportId != null && <Menu.Item
                    name='report'
                    active={activeItem === 'report'}
                >
                    Обращение
                </Menu.Item>}
            </Menu>
            {activeItem == "reports" && <ReportListComponent {...props}/>}
            {activeItem == "responsibles" && <ResponsibleListComponent {...props}/>}
            {activeItem == "report" && <ReportComponent {...props}/>}
        </Segment>
    );
}

interface IReportComponentProps {
    reportId: string;
    backendApi: IBackendApi;
}

function RenderAttachments(attachments: any[], reportId: string, backendApi: IBackendApi) {
    if (attachments.length == 0)
        return null;
    return (<>
        <Header as='h3'>Приложения</Header>
        <Image.Group size='small'>
            {
                attachments.map((a, i) =>
                    <Image bordered key={i}
                           as="a"
                           href={backendApi.getImageLink(reportId, i)}
                           src={backendApi.getImageLink(reportId, i)}/>
                )
            }
        </Image.Group>
    </>);
}

function RenderReport(reportId: string, report: IReport, responsible: IResponsible, backendApi:
    IBackendApi) {
    const chunks = report.reportText.split("[DELIM]").filter(x => x.trim().length != 0);
    console.log(chunks);
    console.log(report.reportText);
    return (
        <Container textAlign="left" fluid>
            <Header as='h2'>{report.subject}</Header>
            <p>
                Квартальный, ответственный за обращение: <b>{responsible.name}</b>
            </p>
            <Header as='h3'>Текст обращения</Header>
            {chunks.map((chunk, i) => <p key={i}>{chunk}</p>)}
            <Header as='h3'>Местоположение</Header>
            <p>
                <a href={`https://yandex.ru/maps/?ll=${report.location.longitude}%2C${report.location.latitude}&` +
                "mode=whatshere&" +
                "whatshere%5Bpoint%5D=60.494202%2C56.809559&" +
                "whatshere%5Bzoom%5D=10&z=14"}>
                    Ссылка на Яндекс.Карты
                </a>
            </p>
            {RenderAttachments(report.attachments, reportId, backendApi)}
        </Container>
    );
}

function ReportComponent(props: IReportComponentProps) {
    const [report, setReport] = useState(null);
    const [responsible, setResponsible] = useState(null);
    const {backendApi, reportId} = props;
    useEffect(() => {
        async function loadAsync() {
            const report = await backendApi.getReport(reportId);
            const responsible = await backendApi.getResponsible(report.responsibleId);
            setResponsible(responsible);
            setReport(report);
        }

        loadAsync();
    }, [reportId]);
    return (
        report != null && responsible != null && RenderReport(reportId, report, responsible, backendApi)
    );
}

function ApplicationComponentInternal(props: IApplicationComponentProps) {
    return (
        <Grid centered columns={2}>
            <Grid.Column>
                <Segment>
                    <Router>
                        <Switch>
                            <Route path="/" exact
                                   render={_ => <MainPageApplicationComponent {...props} reportId={null}
                                                                              activeItem={"reports"}/>}/>
                            <Route path="/reports"
                                   render={_ => <MainPageApplicationComponent {...props} reportId={null}
                                                                              activeItem={"reports"}/>}/>
                            <Route path="/responsibles"
                                   render={_ => <MainPageApplicationComponent {...props} reportId={null}
                                                                              activeItem={"responsibles"}/>}/>
                            <Route path="/report/:reportId"
                                   render={routerProps =>
                                       <MainPageApplicationComponent {...props}
                                                                     activeItem={"report"}
                                                                     reportId={routerProps.match.params.reportId}/>}/>
                        </Switch>
                    </Router>
                </Segment>
            </Grid.Column>
        </Grid>
    );
}

// @ts-ignore
export const ApplicationComponent = withRouter(ApplicationComponentInternal);
