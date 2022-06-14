import { Pipe, PipeTransform } from '@angular/core';
import { ValidationErrors } from '@angular/forms';

@Pipe({
    name: 'formErrors'
})
export class FormErrorsPipe implements PipeTransform {

    transform(errors: ValidationErrors | undefined | null): { key: string, value: any }[] | null {
        if (!errors)
            return null;

        return Object.keys(errors).map(key => ({ key, value: errors[key] }));
    }

}