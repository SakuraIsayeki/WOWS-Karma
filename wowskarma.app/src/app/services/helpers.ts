/**
 * Get the bootstrap color to apply to a Karma metric.
 * @param karma The karma value.
 * @returns The bootstrap color to apply to the karma value.
 *
 *  - `success` if the karma value is positive.
 *  - `danger` if the karma value is negative.
 *  - `warning` if the karma value is zero.
 */
import { ApiRegion } from "../models/ApiRegion";
import { PostFlairs } from "./api/models/post-flairs";
import { AppConfigService } from "./app-config.service";
import { countBalance, parseFlairsEnum } from "./metricsHelpers";

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

