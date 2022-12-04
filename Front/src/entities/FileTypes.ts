export interface SoundFileMeta {
    name: string;
    author: string;
    album: string;
}

export interface TextFileMeta {
    name: string;
}

export enum VideoFileExtension {
    Mp4 = "mp4",
    Mov = "mov",
    Avi = "avi"
}

export interface VideoFileMeta {
    name: string;
    extension: VideoFileExtension;
    studio: string;
    producer: string;
}

export enum FileType {
    sound = 'sound',
    text = 'text',
    video = 'video'
}
