import { Component, input, Input } from "@angular/core";
import { FormControl } from "@angular/forms";
import { formatBytesSize } from "../../services/helpers";
import { CommonModule } from "@angular/common";

@Component({
    selector: "form-errors",
    templateUrl: "./form-errors.component.html",
    imports: [
        CommonModule
    ]
})
export class FormErrorsComponent {
    control = input<FormControl>();

    formatBytesSize(bytesSize: number) {
        return formatBytesSize(bytesSize);
    }
}