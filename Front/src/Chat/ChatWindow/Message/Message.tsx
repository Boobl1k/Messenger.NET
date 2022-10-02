import IMessage from "../../../entities/IMessage";
import {useState} from "react";

interface MessageProps {
    message: IMessage
}

export function Message({message}: MessageProps) {
    return (
        <div style={{background: "#eee", borderRadius: '5px', padding: '0 10px'}}>
            <p><strong>{message.userName}</strong> says:</p>
            <p>{message.text}</p>
        </div>
    );
}