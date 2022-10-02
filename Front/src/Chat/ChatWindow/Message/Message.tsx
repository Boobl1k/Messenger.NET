import IMessage from "../../../entities/IMessage";

interface MessageProps {
    message: IMessage
}

export default function Message({message}: MessageProps) {
    return (
        <div style={{background: "#eee", borderRadius: '5px', padding: '0 10px'}}>
            <p><b>{message.userName}</b> <i>{message.dateTime.toString()}</i>:</p>
            <p>{message.text}</p>
        </div>
    );
}