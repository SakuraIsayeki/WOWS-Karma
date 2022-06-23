import { Pipe, PipeTransform } from '@angular/core';
import { PostFlairs } from "../api/models/post-flairs";
import { getPostBorderColor } from "../helpers";

@Pipe({
  name: 'postBorderColor'
})
export class PostBorderColorPipe implements PipeTransform {

  transform(value: PostFlairs | number): unknown {
      return getPostBorderColor({ flairs: value });
  }
}
