import React, {useState, useEffect} from 'react';
import {HubConnection, HubConnectionBuilder} from '@microsoft/signalr';
import {v4 as uuidv4} from 'uuid';
import ChatWindow from "./ChatWindow/ChatWindow";
import ChatInput from "./ChatInput/ChatInput";
import IMessage from "../entities/IMessage";
import axios from "axios";
import {Button} from "@mui/material";

export default function Chat() {
    const [chat, setChat] = useState<IMessage[]>([]);
    const [connection, setConnection] = useState<null | HubConnection>(null);

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
                    connection.on('ReceiveMessage', (message: IMessage) => {
                        setChat(prev => [...prev, message]);
                    });
                })
                .catch(error => console.log('Connection failed: ', error));
        }
    }, [connection]);

    useEffect(() => {
        axios.get<IMessage[]>('http://localhost:5001/api/messages').then(res => setChat(res.data));
    }, [])

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
                        //await axios.post<IMessage>('http://localhost:5001/api/messages', chatMessage);
                    } catch (error) {
                        console.log('Publishing in MassTransit failed.', error);
                    }
                });
    }

    return (
        <div>
            <Button onClick={async () => {
                await axios.delete('http://localhost:5001/api/messages');
                setChat([]);
            }}>
                Reset
            </Button>
            <ChatInput sendMessage={sendMessage}/>
            <hr/>
            <ChatWindow chat={chat}/>
        </div>
    );
}