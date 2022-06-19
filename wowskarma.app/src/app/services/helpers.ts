import { AbstractControl, FormArray, FormControl, FormGroup } from "@angular/forms";
import { ApiRegion } from "../models/ApiRegion";
import { PostFlairs } from "./api/models/post-flairs";
import { AppConfigService } from "./app-config.service";
import { countBalance, parseFlairsEnum } from "./metricsHelpers";

/**
 * Get the bootstrap color to apply to a Karma metric.
 * @param karma The karma value.
 * @returns The bootstrap color to apply to the karma value.
 *
 *  - `success` if the karma value is positive.
 *  - `danger` if the karma value is negative.
 *  - `warning` if the karma value is zero.
 */
export function getKarmaColor(karma: number): "success" | "danger" | "warning" {
    if (karma > 0) {
        return "success";
    }
    if (karma < 0) {
        return "danger";
    }
    return "warning";
}

/**
 * Gets the link to a player's profile on wows-numbers.com.
 * @param playerIdNamePair The player's ID and username.
 * @returns The link to the player's profile.
 * @see https://wows-numbers.com
 */
export function getWowsNumbersPlayerLink({ id, username }: { id: number; username: string }): string | undefined {
    // Get the base site URL, specific to the ApiRegion.
    switch (AppConfigService.getApiRegionFromLocation()) {
        case ApiRegion.EU:
            return `https://wows-numbers.com/player/${id},${username}`;
        case ApiRegion.NA:
            return `https://na.wows-numbers.com/player/${id},${username}`;
        case ApiRegion.CIS:
            return `https://ru.wows-numbers.com/player/${id},${username}`;
        case ApiRegion.SEA:
            return `https://asia.wows-numbers.com/player/${id},${username}`;
        default:
            return undefined;
    }
}

/**
 * Gets the link to a clan's profile on wows-numbers.com.
 * @param clanInfo The clan's ID, tag, and name.
 * @returns The link to the clan's profile.
 * @see https://wows-numbers.com
 */
export function getWowsNumbersClanLink({ id, tag, name }: { id: number; tag: string; name: string }): string | undefined {
    // Get the base site URL, specific to the ApiRegion.
    switch (AppConfigService.getApiRegionFromLocation()) {
        case ApiRegion.EU:
            return `https://wows-numbers.com/clan/${id},${tag}-${name}`;
        case ApiRegion.NA:
            return `https://na.wows-numbers.com/clan/${id},${tag}-${name}`;
        case ApiRegion.CIS:
            return `https://ru.wows-numbers.com/clan/${id},${tag}-${name}`;
        case ApiRegion.SEA:
            return `https://asia.wows-numbers.com/clan/${id},${tag}-${name}`;
        default:
            return undefined;
    }
}

/**
 * Gets the border color of a post, based on its flairs
 * @param post The post flairs
 * @returns The border color of the post
 */
export function getPostBorderColor({ flairs }: { flairs: PostFlairs | undefined }) {
    return getKarmaColor(countBalance(parseFlairsEnum(flairs ?? 0x00)));
}

/**
 * Sorts objects by their creation date/time.
 */
export function sortByCreationDate(
    a: { createdAt?: Date | string | null },
    b: { createdAt?: Date | string | null },
    reverse = false,
) {

    const aDate = a.createdAt instanceof Date ? a.createdAt : new Date(a.createdAt ?? "");
    const bDate = b.createdAt instanceof Date ? b.createdAt : new Date(b.createdAt ?? "");
    return reverse ? bDate.getTime() - aDate.getTime() : aDate.getTime() - bDate.getTime();
}

/**
 * Returns a human-readable string representation of a number of bytes.
 * @param bytes The number of bytes.
 * @param decimals The number of decimals to show past the decimal point.
 * @returns A human-readable string representation of the number of bytes.
 * @see https://stackoverflow.com/questions/15900485/correct-way-to-convert-size-in-bytes-to-kb-mb-gb-in-javascript
 **/
export function formatBytesSize(bytes: number | undefined | null, decimals = 2) {
    if (bytes === undefined || bytes === null) {
        return "Unknown size";
    }

    if (bytes === 0) {
        return "0 Bytes";
    }

    const k = 1024;
    const dm = decimals < 0 ? 0 : decimals;
    const sizes = ["Bytes", "KB", "MB", "GB", "TB", "PB", "EB", "ZB", "YB"];

    const i = Math.floor(Math.log(bytes) / Math.log(k));

    return parseFloat((bytes / Math.pow(k, i)).toFixed(dm)) + " " + sizes[i];
}

export function markTouchedDirtyAndValidate(
    ctrl: AbstractControl | null | undefined,
    onlySelf: boolean = true,
    updateValidity: boolean = true,
): void {
    if (ctrl instanceof FormControl) {
        ctrl.markAsTouched({ onlySelf });
        ctrl.markAsDirty({ onlySelf });

        if (updateValidity) {
            ctrl.updateValueAndValidity({ onlySelf, emitEvent: true });
        }
    } else if (ctrl instanceof FormArray) {
        const formArray = ctrl;

        for (let i = 0; i < formArray.length; i++) {
            markTouchedDirtyAndValidate(formArray.at(i), onlySelf, updateValidity);
        }
        formArray.markAsTouched({ onlySelf });
        formArray.markAsDirty({ onlySelf });
        if (updateValidity) {
            formArray.updateValueAndValidity({ onlySelf, emitEvent: true });
        }
    } else if (ctrl instanceof FormGroup) {
        const formGroup = ctrl;

        Object.keys(formGroup.controls).forEach(key => {
            const subControl = formGroup.get(key);

            markTouchedDirtyAndValidate(subControl, onlySelf, updateValidity);
        });

        formGroup.markAsTouched({ onlySelf });
        formGroup.markAsDirty({ onlySelf });
        if (updateValidity) {
            formGroup.updateValueAndValidity({ onlySelf, emitEvent: true });
        }
    }
}