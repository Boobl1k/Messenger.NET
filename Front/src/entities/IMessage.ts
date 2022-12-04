export default interface IMessage {
    id: string;
    text: string;
    dateTime: Date;
    userName: string;
    adminName: string;
    sentByAdmin: boolean;
}
