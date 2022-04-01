function surprise(): void {    
    let modal = new bootstrap.Modal(document.getElementById('staticBackdrop'));
    modal.show();
    
    let video: HTMLVideoElement = document.getElementById("weegee") as HTMLVideoElement;
    (video as HTMLVideoElement).play();
    
    setFullscreen();
}

function setFullscreen(): void {
    let video: HTMLVideoElement = document.getElementById("weegee") as HTMLVideoElement;
    video.requestFullscreen();
}

