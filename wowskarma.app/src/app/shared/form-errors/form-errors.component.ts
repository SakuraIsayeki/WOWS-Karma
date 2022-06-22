import { Component, Input } from "@angular/core";
import { FormControl } from "@angular/forms";
import { formatBytesSize } from "../../services/helpers";

@Component({
    selector: "form-errors",
    templateUrl: "./form-errors.component.html",
})
export class FormErrorsComponent {

    @Input()
    control?: FormControl;

    formatBytesSize(bytesSize: number) {
        return formatBytesSize(bytesSize);
    }
}