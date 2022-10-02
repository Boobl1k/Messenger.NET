import React, {useState} from 'react';

interface ChatInputProps {
    sendMessage: (user: string, message: string) => void;
}

export function ChatInput({sendMessage}: ChatInputProps) {
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
            <label htmlFor="user">User:</label>
            <br/>
            <input
                id="user"
                name="user"
                value={user}
                onChange={userUpdateHandler}
            />
            <br/>
            <label htmlFor="message">Message:</label>
            <br/>
            <input
                type="text"
                id="message"
                name="message"
                value={message}
                onChange={messageUpdateHandler}
            />
            <br/><br/>
            <button>Submit</button>
        </form>
    )
}
