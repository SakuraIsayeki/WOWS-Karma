window.addEventListener('DOMContentLoaded', setThemeDisplay);

function setThemeDisplay() {
	if (localStorage['theme'] === 'light') {
		document.querySelector("#light-theme").removeAttribute('disabled');
		document.querySelector("#dark-theme").setAttribute('disabled', true);
	}
	else {
		document.querySelector("#light-theme").setAttribute('disabled', true);
		document.querySelector("#dark-theme").removeAttribute('disabled');
	}
}

function setTheme() {
	//Invert theme
	var theme = localStorage['theme'] === 'light' ? 'dark' : 'light'; 

	localStorage['theme'] = theme;
	document.querySelector("#icon-theme-selector").setAttribute('class', getThemeSelectorIcon());
	setThemeDisplay();
}

function getThemeSelectorIcon() {
	if (localStorage['theme'] === 'light') {
		return 'bi bi-moon';
	}
	else {
		return 'bi bi-sun';
	}
}