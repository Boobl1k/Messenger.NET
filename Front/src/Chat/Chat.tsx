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
import {useNavigate, useParams} from "react-router-dom";

type Props = {
    isAdmin: boolean,
}

export default function Chat(props: Props) {
    const [chat, setChat] = useState<IMessage[]>([]);
    const [connection, setConnection] = useState<null | HubConnection>(null);
    const {userName, adminName} = useParams();
    const navigate = useNavigate();

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
                    connection.on('ReceiveMessage', (u: string, message: IMessage) => {
                        if (u === userName)
                            setChat(prev => [...prev, message]);
                    });
                    connection.on('GoWait', (a: string) => {
                        if(a === adminName){
                            if(props.isAdmin)
                                navigate(`/wait/admin/${adminName}`);
                            else
                                navigate(`/`);
                        }
                        
                    })
                })
                .catch(error => console.log('Connection failed: ', error));
        }
    }, [connection]);

    useEffect(() => {
        axios.get<IMessage[]>('messages', {params: {username: userName}}).then(res => setChat(res.data));
    }, [])

    const sendMessage = async (text: string) => {
        if (!userName || !adminName) {
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
                .send("SendMessage", userName, chatMessage)
                .catch((e) => {
                    console.error(e);
                    console.log('Publishing in SignalR failed')
                });
    }

    const freeAdmin = async () => {
        if (connection)
            await connection
                .send("FreeAdmin", adminName)
                .catch((e) => {
                    console.error(e);
                    console.log('Cannot free the admin');
                });
    }

    const onCloseChat = async () => {
        await freeAdmin();
        navigate('/');
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
                <Button onClick={onCloseChat}>
                    Close chat
                </Button>
                <ChatWindow chat={chat}/>
                <hr/>
                <ChatInput sendMessage={sendMessage}/>
            </div>
            {/*<FileUploader/>*/}
        </>
    );
}