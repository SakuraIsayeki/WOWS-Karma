function setThemeDisplay(theme: string = localStorage['theme']): void {
	if (theme === 'light') {
		document.querySelector("#light-theme").removeAttribute('disabled');
		document.querySelector("#dark-theme").setAttribute('disabled', '');
	}
	else {
		document.querySelector("#dark-theme").removeAttribute('disabled');
		document.querySelector("#light-theme").setAttribute('disabled', '');
	}
}

function getThemeSelectorIcon(theme: string): string {
	if (theme === 'light') {
		return 'bi bi-moon';
	}
	else {
		return 'bi bi-sun';
	}
}

function setTheme(theme: string = localStorage['theme']): void {
	//Invert theme
	theme = theme === 'light' ? 'dark' : 'light';

	localStorage['theme'] = theme;
	document.querySelector("#icon-theme-selector").setAttribute('class', getThemeSelectorIcon(theme));
	setThemeDisplay();
}

window.addEventListener('DOMContentLoaded', () => setThemeDisplay);