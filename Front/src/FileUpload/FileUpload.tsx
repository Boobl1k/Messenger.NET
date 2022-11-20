import axios from "axios";
import './fileUpload.css'
import React, {useState} from "react";
import {BASE_URL} from "../config";
import {v4 as uuidv4} from 'uuid';
import {FileType, SoundFileMeta, TextFileMeta, VideoFileExtension, VideoFileMeta} from "../entities/FileTypes";

export default function FileUploader() {
    const [file, setFile] = useState<null | File>(null);
    const [fileType, setFileType] = useState<FileType>(FileType.text);
    const [soundFileMeta, setSoundFileMeta] = useState<SoundFileMeta>({
        id: '',
        album: 'album',
        author: 'author',
        name: 'name'
    });
    const [textFileMeta, setTextFileMeta] = useState<TextFileMeta>({id: '', name: 'name'});
    const [videoFileMeta, setVideoFileMeta] = useState<VideoFileMeta>({
        id: '',
        studio: 'studio',
        producer: 'producer',
        extension: VideoFileExtension.Mp4,
        name: 'name'
    });

    const fileInputHandler = (event: React.ChangeEvent<HTMLInputElement>) => {
        setFile((event.target.files)![0]);
    };

    const onSetFileType = (event: React.ChangeEvent<HTMLSelectElement>) => {
        setFileType(event.target.value as FileType);
    }

    const sendFile = async (id: string) => {
        const formData = new FormData();

        if (file) {
            formData.append('file', file);

            await axios.post(`${BASE_URL}api/files/?id=${id}`, formData)
                .then((event) => {
                    console.log("success");
                })
                .catch((e) => {
                    console.error('Error', e);
                });
        }
    }

    const sendMeta = async (id: string) => {
        let meta: SoundFileMeta | TextFileMeta | VideoFileMeta;
        switch (fileType) {
            case FileType.sound:
                meta = soundFileMeta;
                break;
            case FileType.text:
                meta = textFileMeta;
                break;
            case FileType.video:
                meta = videoFileMeta;
                break;
        }

        await axios.post(`${BASE_URL}api/FileMetadata/${fileType}/`, {...meta, id})
            .then(() => {
                console.log("success");
            })
            .catch((e) => {
                console.error('Error', e);
            });

    }

    const onSubmit = async (event: any) => {
        event.preventDefault();

        const id = uuidv4();

        await sendFile(id);
        await sendMeta(id);
    };

    const generateMetaEditor = (type: FileType) => {
        switch (type) {
            case FileType.sound:
                const onSetSoundFileMeta = (key: keyof SoundFileMeta) =>
                    (e: React.ChangeEvent<HTMLInputElement>) =>
                        setSoundFileMeta((oldMeta) => ({
                            ...oldMeta,
                            [key]: e.target.value
                        }));
                return (
                    <>
                        <label>Name:</label>
                        <input
                            value={soundFileMeta.name}
                            onChange={onSetSoundFileMeta('name')}
                            className="form-control"/>
                        <label>Author:</label>
                        <input
                            value={soundFileMeta.author}
                            onChange={onSetSoundFileMeta('author')}
                            className="form-control"
                        />
                        <label>Album:</label>
                        <input
                            value={soundFileMeta.album}
                            onChange={onSetSoundFileMeta('album')}
                            className="form-control"
                        />
                    </>
                );
            case FileType.text:
                const onSetTextFileMeta = (key: keyof TextFileMeta) =>
                    (e: React.ChangeEvent<HTMLInputElement>) =>
                        setTextFileMeta((oldMeta) => ({
                            ...oldMeta,
                            [key]: e.target.value
                        }));
                return (
                    <>
                        <label>Name:</label>
                        <input
                            value={textFileMeta.name}
                            onChange={onSetTextFileMeta('name')}
                            className="form-control"/>
                    </>
                );
            case FileType.video:
                const onSetVideoFileMeta = (key: keyof VideoFileMeta) =>
                    (e: React.ChangeEvent<HTMLInputElement | HTMLSelectElement>) =>
                        setVideoFileMeta((oldMeta) => ({
                            ...oldMeta,
                            [key]: e.target.value
                        }));
                return (
                    <>
                        <label>Name:</label>
                        <input
                            value={videoFileMeta.name}
                            onChange={onSetVideoFileMeta('name')}
                            className="form-control"/>
                        <label>Studio:</label>
                        <input
                            value={videoFileMeta.studio}
                            onChange={onSetVideoFileMeta('studio')}
                            className="form-control"
                        />
                        <label>Producer:</label>
                        <input
                            value={videoFileMeta.producer}
                            onChange={onSetVideoFileMeta('producer')}
                            className="form-control"
                        />
                        <label>Extension:</label>
                        <select
                            value={VideoFileExtension.Mp4}
                            onChange={onSetVideoFileMeta('extension')}
                            className="form-control"
                        >
                            <option value={VideoFileExtension.Mp4}>{VideoFileExtension.Mp4}</option>
                            <option value={VideoFileExtension.Avi}>{VideoFileExtension.Avi}</option>
                            <option value={VideoFileExtension.Mov}>{VideoFileExtension.Mov}</option>
                        </select>
                    </>
                );
        }
    }

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
                <label>Type:</label>
                <select
                    value={fileType}
                    onChange={onSetFileType}
                    className="form-control"
                >
                    <option value={FileType.text}>Text</option>
                    <option value={FileType.sound}>Sound</option>
                    <option value={FileType.video}>Video</option>
                </select>
                {generateMetaEditor(fileType)}
            </div>

            <button>Submit</button>
        </form>
    );
}