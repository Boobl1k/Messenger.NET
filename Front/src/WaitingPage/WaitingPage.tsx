import {useNavigate, useParams} from "react-router-dom";
import {useEffect, useState} from "react";
import {HubConnection, HubConnectionBuilder} from "@microsoft/signalr";
import {BASE_URL} from "../config";
import axios from "../axios";

type Props = {
    isAdmin: boolean;
}

export default function WaitingPage(props: Props) {
    const [connection, setConnection] = useState<null | HubConnection>(null);
    const {name} = useParams();
    const navigate = useNavigate();

    const [getAdmin, setGetAdmin] = useState(false);

    useEffect(() => {
        if (props.isAdmin) {
            const connect = new HubConnectionBuilder()
                .withUrl(BASE_URL + 'chat')
                .withAutomaticReconnect()
                .build();

            setConnection(connect);
        } else {
            setGetAdmin(true);
        }
    }, []);

    useEffect(() => {
        if (props.isAdmin) {
            if (connection) {
                connection
                    .start()
                    .then(async () => {
                        connection.on('GoChat', (a, userName) => {
                            if (a === name)
                                navigate(`/chat/admin/${userName}/${name}`);
                        });
                        axios.post('/admin/free', {adminName: name});
                    })
                    .catch(error => console.log('Connection failed: ', error));
            }
        }
    }, [connection]);

    useEffect(() => {
        if (getAdmin) {
            axios.get<{ adminName: string }>('/admin/find', {params: {userName: name}})
                .then(res => navigate(`/chat/user/${name}/${res.data.adminName}`));
        }
    }, [getAdmin]);

    return (
        <>
            <h1>Hello, {name}!</h1>
            {props.isAdmin ?
                <p>You are in queue</p> :
                <p>Searching an admin for you</p>}
        </>
    );
}