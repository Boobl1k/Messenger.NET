import axios from "../axios";
import './fileUpload.css'
import React, {useState} from "react";
import {v4 as uuidv4} from 'uuid';
import {FileType, SoundFileMeta, TextFileMeta, VideoFileExtension, VideoFileMeta} from "../entities/FileTypes";

export default function FileUploader() {
    const [file, setFile] = useState<null | File>(null);
    const [fileType, setFileType] = useState<FileType>(FileType.text);
    const [soundFileMeta, setSoundFileMeta] = useState<SoundFileMeta>({
        album: 'album',
        author: 'author',
        name: 'name'
    });
    const [textFileMeta, setTextFileMeta] = useState<TextFileMeta>({name: 'name'});
    const [videoFileMeta, setVideoFileMeta] = useState<VideoFileMeta>({
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

    const sendFile = async (id: string, file: File) => {
        const formData = new FormData();

        formData.append('file', file);

        await axios.post(`files/?id=${id}`, formData)
            .then(() => {
                console.log("success");
            })
            .catch((e) => {
                console.error('Error', e);
            });
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

        await axios.post(`fileMetadata/${fileType}/`, {...meta, id})
            .then(() => {
                console.log("success");
            })
            .catch((e) => {
                console.error('Error', e);
            });

    }

    const onSubmit = async (event: any) => {
        event.preventDefault();

        if (file) {
            const id = uuidv4();

            await sendFile(id, file);
            await sendMeta(id);
        } else alert('attach your file');
    };

    const generateMetaEditor = (type: FileType) => {
        function generateInput<T>(key: keyof T & string, setter: (a: (value: T) => T) => void, value: T) {
            const onSet = (key: keyof T) =>
                (e: React.ChangeEvent<HTMLInputElement | HTMLSelectElement>) =>
                    setter((oldMeta) => ({
                        ...oldMeta,
                        [key]: e.target.value
                    }));
            return [
                <label>{key[0].toUpperCase() + key.slice(1)}</label>,
                <input value={value[key] as string}
                       onChange={onSet(key)}
                       className="form-control"/>];
        }

        switch (type) {
            case FileType.sound:
                const soundKeys: (keyof SoundFileMeta)[] = ['name', 'author', 'album'];
                return soundKeys.map(key => generateInput<SoundFileMeta>(key, setSoundFileMeta, soundFileMeta));
            case FileType.text:
                return generateInput<TextFileMeta>('name', setTextFileMeta, textFileMeta);
            case FileType.video:
                const onSetVideoFileMeta = (key: keyof VideoFileMeta) =>
                    (e: React.ChangeEvent<HTMLInputElement | HTMLSelectElement>) =>
                        setVideoFileMeta((oldMeta) => ({
                            ...oldMeta,
                            [key]: e.target.value
                        }));
                const videoKeys: (keyof VideoFileMeta)[] = ['name', 'studio', 'producer'];
                return (
                    <>
                        {videoKeys.map(key => generateInput<VideoFileMeta>(key, setVideoFileMeta, videoFileMeta))}
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