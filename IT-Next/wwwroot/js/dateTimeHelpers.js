function GetDateTimeObjectAsClientZone(utcDateTimeString, ignoreTimeZone = false) {
	const dateTime = new Date(utcDateTimeString);
	if (ignoreTimeZone)
		return dateTime;

	const differenceMinutes = 0 - new Date().getTimezoneOffset();
	dateTime.setMinutes(dateTime.getMinutes() + differenceMinutes);
	return dateTime;
}

function FormatDateTime(dateObject, format) {
	let dateTimePoints = [];
	dateTimePoints["yyyy"] = dateObject.getFullYear();
	dateTimePoints["yy"] = dateTimePoints["yyyy"].toString().substr(2, 2);
	dateTimePoints["MM"] = (dateObject.getMonth() + 1) < 10 ? `0${dateObject.getMonth() + 1}` : (dateObject.getMonth() + 1);
	dateTimePoints["M"] = dateTimePoints["MM"] < 10 ? dateTimePoints["MM"].toString().substr(1, 1) : dateTimePoints["MM"];
	dateTimePoints["dd"] = dateObject.getDate() < 10 ? `0${dateObject.getDate()}` : dateObject.getDate();
	dateTimePoints["d"] = dateTimePoints["dd"] < 10 ? dateTimePoints["dd"].toString().substr(1, 1) : dateTimePoints["dd"];
	dateTimePoints["HH"] = dateObject.getHours() < 10 ? `0${dateObject.getHours()}` : dateObject.getHours();
	dateTimePoints["H"] = dateTimePoints["HH"] < 10 ? dateTimePoints["HH"].toString().substr(1, 1) : dateTimePoints["HH"];
	let hhValue, aValue;
	if (dateTimePoints["HH"] === 0 || dateTimePoints["HH"] === 24) {
		hhValue = 12;
		aValue = "AM";
	}
	else if (dateTimePoints["HH"] > 12) {
		hhValue = dateTimePoints["HH"] - 12;
		aValue = "PM";
	} else {
		hhValue = dateTimePoints["HH"];
		aValue = "AM";
	}
	dateTimePoints["hh"] = hhValue.toString().length < 2 && hhValue < 10 ? `0${hhValue}` : hhValue;
	dateTimePoints["a"] = aValue;
	dateTimePoints["h"] = dateTimePoints["hh"] < 10 ? dateTimePoints["hh"].toString().substr(1, 1) : dateTimePoints["hh"];
	dateTimePoints["mm"] = dateObject.getMinutes().toString().length < 2 && dateObject.getMinutes() < 10 ? `0${dateObject.getMinutes()}` : dateObject.getMinutes();
	dateTimePoints["m"] = dateTimePoints["mm"] < 10 ? dateTimePoints["mm"].toString().substr(1, 1) : dateTimePoints["mm"];
	const neededValues = format.split(/[-:/ ]/);
	for (let neededValue of neededValues) {
		const regex = new RegExp(`(${neededValue})`, "g");
		format = format.replace(regex, dateTimePoints[neededValue]);
	}
	return format;
}