function checkUsername() {
  const email = document.getElementById("log_username").value;
  const errorElement = document.getElementById("log_errorUsername");
  errorElement.innerHTML = "";

  if (email === "") {
    errorElement.innerHTML = "Впишите никнейм";
    return false;
  }
  else { return true; }
}

function checkPassword() {
  const password = document.getElementById("log_password").value;
  const errorElement = document.getElementById("log_errorPassword");
  errorElement.innerHTML = "";

  if (password === "") {
    errorElement.innerHTML = "Впишите пароль";
    return false;
  }
  return true;
}

function clearForm() {
  const textInputs = document.querySelectorAll("input[type='text'], input[type='email'], input[type='password'], input[type='number']");
  textInputs.forEach(input => input.value = "");
  const radioButtons = document.querySelectorAll("input[type='radio']");
  radioButtons.forEach(radio => radio.checked = false);
  const dropdowns = document.querySelectorAll("select");
  dropdowns.forEach(select => select.selectedIndex = 0);
  const errorMessages = document.querySelectorAll("span.text-danger");
  errorMessages.forEach(span => span.innerHTML = "");
}

function validateLoginForm() {
  //let isFormOK = true;
  //isFormOK = checkPassword() && isFormOK;
  //isFormOK = checkEmail() && isFormOK;
  return checkUsername() && checkPassword();
}
