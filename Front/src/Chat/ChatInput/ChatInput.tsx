import React, {useState} from 'react';
import {Input, InputLabel} from "@mui/material";

interface ChatInputProps {
    sendMessage: (user: string, message: string) => void;
}

export default function ChatInput({sendMessage}: ChatInputProps) {
    const [user, setUser] = useState('');
    const [message, setMessage] = useState('');

    const submitHandler = (event: React.FormEvent) => {
        event.preventDefault();

        const isUserProvided = user && user !== '';
        const isMessageProvided = message && message !== '';

        if (isUserProvided && isMessageProvided) {
            sendMessage(user, message);
        } else {
            alert('Please insert an user and a message.');
        }
    }

    const userUpdateHandler = (event: React.ChangeEvent<HTMLInputElement>) => {
        setUser(event.target.value);
    }

    const messageUpdateHandler = (event: React.ChangeEvent<HTMLInputElement>) => {
        setMessage(event.target.value);
    }

    return (
        <form onSubmit={submitHandler}>
            <InputLabel htmlFor="user">User:</InputLabel>
            <Input
                id="user"
                name="user"
                value={user}
                onChange={userUpdateHandler}
            />
            <br/><br/>
            <InputLabel htmlFor="message">Message:</InputLabel>
            <Input
                type="text"
                id="message"
                name="message"
                value={message}
                onChange={messageUpdateHandler}
            />
            <br/><br/>
            <Input type="Submit" value="Submit"></Input>
        </form>
    )
}
