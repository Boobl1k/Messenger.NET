import React, {useState, useEffect, useRef} from 'react';
import {HubConnectionBuilder} from '@microsoft/signalr';
import { v4 as uuidv4 } from 'uuid';
import {ChatWindow} from "./ChatWindow/ChatWindow";
import {ChatInput} from "./ChatInput/ChatInput";
import IMessage from "../entities/IMessage";
import axios from "axios";


export function Chat() {
    const [chat, setChat] = useState<IMessage[]>([]);

    useEffect(() => {
        const connection = new HubConnectionBuilder()
            .withUrl('http://localhost:5001/chat')
            .withAutomaticReconnect()
            .build();

        connection.start()
            .then(result => {
                console.log('Connected!');

                connection.on('ReceiveMessage', message => {
                    const updatedChat = [...chat];
                    updatedChat.push(message);

                    setChat(updatedChat);
                });
            })
            .catch(e => console.log('Connection failed: ', e));
    }, []);

    const sendMessage = async (userName: string, text: string) => {
        const chatMessage: IMessage = {
            id: uuidv4(),
            userName: userName,
            text: text,
            dateTime: new Date()
        };

        try {
            await fetch('http://localhost:5001/api/messages', {
                method: 'POST',
                body: JSON.stringify(chatMessage),
                headers: {
                    'Content-Type': 'application/json'
                }
            });
            // const response = await axios.post<IMessage>('http://localhost:5001/api/messages', chatMessage);
            // console.log(response)

        } catch (error) {
            console.log('Sending message failed.', error);
        }
    }

    return (
        <div>
            <ChatInput sendMessage={sendMessage}/>
            <hr/>
            <ChatWindow chat={chat}/>
        </div>
    );
}