import React, {useEffect, useState} from 'react';
import './App.css';
import {List, Button, ListItem, ListItemText, Box, Divider, TextField} from "@mui/material";
import Message from "./entities/message";

const BASE_URL = 'https://localhost:5002/';
const fetchFromBack = (url: string) => fetch(BASE_URL + url).then(res => res.json());
const postToBack = (url: string, data: any) => fetch(
    BASE_URL + url,
    {
        method: 'POST',
        body: JSON.stringify(data),
        headers: {'content-type': 'application/json'}
    });

function App() {
    const [messages, setMessages] = useState([] as Message[]);
    const [userName, setUserName] = useState('');
    const [inputMessage, setInputMessage] = useState('');

    useEffect(() => {
        fetchFromBack('messages').then(setMessages);
    }, [])

    return (
        <div className="App">
            <header className="App-header">
                <Box sx={{width: '100%', maxWidth: 500, bgcolor: 'background.paper'}}>
                    <nav aria-label="secondary mailbox folders">
                        <List sx={{
                            width: '100%',
                            bgcolor: 'background.paper',
                            position: 'relative',
                            overflow: 'auto',
                            maxHeight: 800,
                            minHeight: 800,
                            '& ul': {padding: 0},
                        }}>
                            {
                                messages.map(m => (
                                    <div style={{display: 'flex'}}>
                                        <ListItem>
                                            <ListItemText primary={m.text} secondary={m.userName}></ListItemText>
                                        </ListItem>
                                        <Divider/>
                                    </div>
                                ))
                            }
                        </List>
                        <TextField variant="outlined" onChange={e => setInputMessage(e.target.value)}></TextField>
                        <Button variant="outlined" onClick={() => postToBack("SendMessage", {
                            Message: inputMessage,
                            UserName: userName
                        })}>Send</Button>
                        <Divider/>
                    </nav>
                </Box>
                <div className="userNameInput">
                    <TextField variant="outlined" onChange={e => setUserName(e.target.value)}/>
                </div>
            </header>
        </div>
    );
}

export default App;
