import React, {useState} from "react";
import {Link} from "react-router-dom";

export default function LoginForm() {
    const [username, setUsername] = useState('');
    
    const onSetUsername = (event: React.ChangeEvent<HTMLInputElement>) => setUsername(event.target.value);

    return <>
        <input type={"text"} onChange={onSetUsername}/>
        <Link to={`chat/admin/${username}`}>Login as admin</Link>
        <Link to={`chat/user/${username}`}>Login as user</Link>
    </>
}