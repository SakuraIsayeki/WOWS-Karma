import { AbstractControl, ValidationErrors } from "@angular/forms";

/*
 * FIXME: Faulty file input validation.
 */

export const replayFileValid = (control: AbstractControl<File | undefined, File | undefined>): ValidationErrors | null => {
    let error: any = {};
    const replayFile = control.parent?.get("replayFile");

    // Check the replay file, if it exists
    if (replayFile?.value) {
        // Check if replay file is larger than 5 MiB
        if (replayFile.value.size > 5242880) {
            error.replayFile.maxsize = 5242880;
        }

        // Check if replay file is a valid type
        const type = "application/octet-stream";
        if (replayFile.value.type !== type) {
            error.replayFile.type = type;
        }

        // Check if extension is .wowsreplay
        if (replayFile.value.name.split(".").pop() !== "wowsreplay") {
            error.replayFile.extension = "wowsreplay";
        }
    }

    return error === {} ? null : error;
};

export const requireReplay = (control: AbstractControl<File | undefined, File | undefined>): ValidationErrors | null => {
    let error: any = {};
    const id = control.parent?.get("id");
    const flairs = control.parent?.get("parsedFlairs");
    const replayFile = control.parent?.get("replayFile");

    // If the flairs are set, and the post is being created, then the replay file is required.
    if (flairs && !id?.value && (flairs.value.performance || flairs.value.teamplay || flairs.value.courtesy) ) {
        // Replay is required. Check for it.
        if (!replayFile?.value) {
            error.replayFile = { required: true };
        }
    }

    return error === {} ? null : error;
};

