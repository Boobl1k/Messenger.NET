import React, {useState, useEffect, useRef} from 'react';
import {HubConnection, HubConnectionBuilder} from '@microsoft/signalr';
import {v4 as uuidv4} from 'uuid';
import ChatWindow from "./ChatWindow/ChatWindow";
import ChatInput from "./ChatInput/ChatInput";
import IMessage from "../entities/IMessage";
import axios from "axios";

export default function Chat() {
    const [chat, setChat] = useState<IMessage[]>([]);
    const [connection, setConnection] = useState<null | HubConnection>(null);
    const latestChat = useRef<IMessage[] | null>(null);
    // latestChat.current = chat;

    useEffect(() => {
        const connect = new HubConnectionBuilder()
            .withUrl('http://localhost:5001/chat')
            .withAutomaticReconnect()
            .build();

        setConnection(connect);
    }, []);

    useEffect(() => {
        if (connection) {
            connection
                .start()
                .then(async () => {
                    console.log('Connected to signalR!');
                    console.log(`Received message, updating chat!`);
                    const response = await axios.get<IMessage[]>('http://localhost:5001/api/messages');
                    latestChat.current = response.data;
                    setChat(latestChat.current);

                    connection.on('ReceiveMessage', (message: IMessage) => {
                        try {
                            const updatedChat = [...(latestChat.current as IMessage[])];
                            updatedChat.push(message);
 
                            setChat(updatedChat);
                        } catch (error) {
                            console.log('Receiving message failed.', error);
                        }
                    });
                })
                .catch(error => console.log('Connection failed: ', error));
        }
    }, [connection]);

    const sendMessage = async (userName: string, text: string) => {
        const chatMessage: IMessage = {
            id: uuidv4(),
            userName: userName,
            text: text,
            dateTime: new Date()
        };

        if (connection)
            await connection
                .send("SendMessage", chatMessage)
                .then(async () => {
                    try {
                        console.log('Published in MassTransit');
                        const response = await axios.post<IMessage>('http://localhost:5001/api/messages', chatMessage);
                        console.log(response)
                    } catch (error) {
                        console.log('Publishing in MassTransit failed.', error);
                    }
                });
    }

    return (
        <div>
            <ChatInput sendMessage={sendMessage}/>
            <hr/>
            <ChatWindow chat={chat}/>
        </div>
    );
}