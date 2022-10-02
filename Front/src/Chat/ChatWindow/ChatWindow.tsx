import React from 'react';
import {Message} from "./Message/Message";
import IMessage from "../../entities/IMessage";

interface ChatWindowProps {
    chat: IMessage[];
}

export function ChatWindow({chat}: ChatWindowProps) {
    return (
        <div>
            {chat.map(message => <Message message={message} key={message.id}/>)}
        </div>
    )
}