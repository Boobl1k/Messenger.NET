import React, {useState, useEffect} from 'react';
import {HubConnection, HubConnectionBuilder} from '@microsoft/signalr';
import {v4 as uuidv4} from 'uuid';
import ChatWindow from "./ChatWindow/ChatWindow";
import ChatInput from "./ChatInput/ChatInput";
import IMessage from "../entities/IMessage";
import axios from "../axios";
import {Button} from "@mui/material";
import {API_URL, BASE_URL} from "../config";
import FileUploader from "../FileUpload/FileUpload";
import {useParams} from "react-router-dom";

type Props = {
    isAdmin: boolean,
}

export default function Chat(props: Props) {
    const [chat, setChat] = useState<IMessage[]>([]);
    const [connection, setConnection] = useState<null | HubConnection>(null);
    const {userName, adminName} = useParams();

    useEffect(() => {
        const connect = new HubConnectionBuilder()
            .withUrl(BASE_URL + 'chat')
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
        axios.get<IMessage[]>('messages', {params: {username: userName}}).then(res => setChat(res.data));
    }, [])

    const sendMessage = async (text: string) => {
        if(!userName || !adminName){
            console.error('there has to be username');
            return;
        }
        const chatMessage: IMessage = {
            id: uuidv4(),
            userName: userName,
            adminName: adminName,
            text: text,
            dateTime: new Date(),
            sentByAdmin: props.isAdmin,
        };

        if (connection)
            await connection
                .send("SendMessage", chatMessage)
                .catch(() => console.log('Publishing in SignalR failed'));
    }

    return (
        <>
            <div className="flex flex-col flex-grow w-full max-w-xl bg-white shadow-xl rounded-lg overflow-hidden">
                <Button onClick={async () => {
                    await axios.delete('messages');
                    setChat([]);
                }}>
                    Reset
                </Button>
                <ChatWindow chat={chat}/>
                <hr/>
                <ChatInput sendMessage={sendMessage}/>
            </div>
            <FileUploader/>
        </>
    );
}