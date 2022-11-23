import axios from "axios";
import './fileUpload.css'
import React, {useState} from "react";
import {BASE_URL} from "../config";
import {v4 as uuidv4} from 'uuid';

export default function FileUploader() {
    const [file, setFile] = useState<null | File>(null);
    const [metaName, setMetaName] = useState<string>('');
    const [metaAuthor, setMetaAuthor] = useState<string>('');
    const [metaAlbum, setMetaAlbum] = useState<string>('');

    const fileInputHandler = (event: React.ChangeEvent<HTMLInputElement>) => {
        setFile((event.target.files)![0]);
    };

    const onSetMetaName = (event: React.ChangeEvent<HTMLInputElement>) => {
        setMetaName(event.target.value);
    };

    const onSetMetaAuthor = (event: React.ChangeEvent<HTMLInputElement>) => {
        setMetaAuthor(event.target.value);
    };

    const onSetMetaAlbum = (event: React.ChangeEvent<HTMLInputElement>) => {
        setMetaAlbum(event.target.value);
    };

    const generateGuid = uuidv4();

    const sendFile = async () => {
        const formData = new FormData();

        if (file) {
            formData.append('file', file);

            await axios.post(`${BASE_URL}api/files/?id=${generateGuid}`, formData)
                .then((event) => {
                    console.log("success");
                })
                .catch((e) => {
                    console.error('Error', e);
                });
        }
    }

    const sendMeta = async () => {
        const formData = new FormData();

        formData.append('Id', generateGuid);
        formData.append('Name', metaName);
        formData.append('Author', metaAuthor);
        formData.append('Album', metaAlbum);

        await axios.post(`${BASE_URL}api/FileMetadata/PostAudioFileMetadata/Sound/`, formData)
            .then((event) => {
                console.log("success");
            })
            .catch((e) => {
                console.error('Error', e);
            });

    }

    const onSubmit = async (event: any) => {
        event.preventDefault();

        await sendFile();
        await sendMeta();
    };

    return (
        <form method='post' action='#' id='#' onSubmit={onSubmit}>
            <div className="form-group files">
                <label>Upload your file</label>
                <input
                    type="file"
                    onChange={fileInputHandler}
                    className="form-control"
                    multiple/>
            </div>

            <div className="form-group">
                <label>Name:</label>
                <input
                    value={metaName}
                    onChange={onSetMetaName}
                    className="form-control"/>
                <label>Author:</label>
                <input
                    value={metaAuthor}
                    onChange={onSetMetaAuthor}
                    className="form-control"
                />
                <label>Album:</label>
                <input
                    value={metaAlbum}
                    onChange={onSetMetaAlbum}
                    className="form-control"
                />
            </div>

            <button>Submit</button>
        </form>
    );
}