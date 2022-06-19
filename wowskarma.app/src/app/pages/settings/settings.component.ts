import 'src/app/services/extensions';
import { ChangeDetectionStrategy, Component, OnInit } from "@angular/core";
import { FormBuilder, FormControl, FormGroup } from "@angular/forms";
import { BehaviorSubject, tap } from "rxjs";
import { map } from "rxjs/operators";
import { UserProfileFlagsDto } from "../../services/api/models/user-profile-flags-dto";
import { ProfileService } from "../../services/api/services/profile.service";
import { AuthService } from "../../services/auth.service";

@Component({
    selector: "app-settings",
    templateUrl: "./settings.component.html",
    changeDetection: ChangeDetectionStrategy.OnPush,
})
export class SettingsComponent implements OnInit {
    currentUserProfile$ = this.authService.profileFlags$;
    changesSaved$ = new BehaviorSubject(false);
    copiedTokenToClipboard$ = new BehaviorSubject(false);
    optOutOnCooldown$ = this.currentUserProfile$.pipe(
        map((profileFlags) => {
            // Check if the user is opted out and if they are on cooldown (1 Week after opting out)
            const cooldownEnd = new Date(profileFlags?.optOutChanged!)?.addDays(7);
            const isOnCooldown = (profileFlags?.optedOut && cooldownEnd && cooldownEnd > new Date(Date.now())) ?? false;
            return { isOnCooldown, cooldownEnd };
        })
    );

    form = this.formBuilder.nonNullable.group({
        id: 0,
        optedOut: false
    });

    constructor(public authService: AuthService, private profileService: ProfileService, private formBuilder: FormBuilder) { }

    ngOnInit(): void {
        throw new Error('Method not implemented.');
    }

    saveChanges() {
        this.profileService.apiProfilePut$Json({ body: this.form.value }).subscribe(() => {
            this.changesSaved$.next(true);
        });
    }

    copyTokenToClipboard() {
        this.authService.authToken$.pipe(
            tap(async (token) => {
                if (token) {
                    await window.navigator.clipboard.writeText(token);
                    this.copiedTokenToClipboard$.next(true);
                }
            })
        );
    }
}