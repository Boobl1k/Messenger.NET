import React, {useState, useEffect, useRef} from 'react';
import {HubConnection, HubConnectionBuilder} from '@microsoft/signalr';
import {v4 as uuidv4} from 'uuid';
import ChatWindow from "./ChatWindow/ChatWindow";
import ChatInput from "./ChatInput/ChatInput";
import IMessage from "../entities/IMessage";

export default function Chat() {
    const [chat, setChat] = useState<IMessage[]>([]);
    const [connection, setConnection] = useState<null | HubConnection>(null);
    const latestChat = useRef<IMessage[] | null>(null);

    latestChat.current = chat;
    
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
                .then(() => {
                    console.log('Connected to signalR!');

                    connection.on('ReceiveMessage', message => {
                        const updatedChat = [...(latestChat.current as IMessage[])];
                        console.log(`updatedChat before: ${JSON.stringify(updatedChat)}`);
                        updatedChat.push(message);
                        console.log(`updatedChat after: ${JSON.stringify(updatedChat)}`);
                        console.log(`pushed message: ${JSON.stringify(message)}`);
                        setChat(updatedChat);
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
            await connection.send("SendMessage", chatMessage);

        // try {
        //     await fetch('http://localhost:5001/api/messages', {
        //         method: 'POST',
        //         body: JSON.stringify(chatMessage),
        //         headers: {
        //             'Content-Type': 'application/json'
        //         }
        //     });
        //     // const response = await axios.post<IMessage>('http://localhost:5001/api/messages', chatMessage);
        //     // console.log(response)
        //
        // } catch (error) {
        //     console.log('Sending message failed.', error);
        // }
    }

    return (
        <div>
            <ChatInput sendMessage={sendMessage}/>
            <hr/>
            <ChatWindow chat={chat}/>
        </div>
    );
}