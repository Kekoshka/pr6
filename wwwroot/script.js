// Конфигурация API
const API_BASE_URL = 'https://localhost:7142/api';
let currentRequestId = null;
let pendingRequest = null;
let currentTokenPair = null;




// Утилиты
function showLoader(show = true) {
    document.getElementById('loader').classList.toggle('active', show);
}

function showAlert(message, type = 'error') {
    // Удаляем старые алерты
    document.querySelectorAll('.alert').forEach(alert => alert.remove());

    const alertDiv = document.createElement('div');
    alertDiv.className = `alert ${type}`;
    alertDiv.textContent = message;

    const container = document.querySelector('.tab-content.active');
    container.insertBefore(alertDiv, container.firstChild);

    setTimeout(() => {
        alertDiv.remove();
    }, 5000);
}

function switchTab(tabName) {
    // Обновляем активные табы
    document.querySelectorAll('.tab').forEach(tab => {
        tab.classList.toggle('active', tab.dataset.tab === tabName);
    });

    document.querySelectorAll('.tab-content').forEach(content => {
        content.classList.toggle('active', content.id === `${tabName}Tab`);
    });

    // Сбрасываем шаги форм
    showStep(tabName, 1);
}

function showStep(process, step) {
    // Скрываем все шаги в активном табе
    const activeTab = document.querySelector('.tab-content.active');
    activeTab.querySelectorAll('.form-step').forEach(step => {
        step.classList.remove('active');
    });

    // Показываем нужный шаг
    const stepElement = document.getElementById(`${process}Step${step}`);
    if (stepElement) {
        stepElement.classList.add('active');
    }
}

// Валидация пароля
function validatePassword() {
    const password = document.getElementById('registerPassword').value;
    const rules = {
        length: password.length >= 8,
        uppercase: /[A-Z]/.test(password),
        lowercase: /[a-z]/.test(password),
        special: /[!@#$%^&*()_+\-=\[\]{};':"\\|,.<>\/?]/.test(password)
    };

    // Обновляем отображение правил
    Object.keys(rules).forEach(rule => {
        const element = document.getElementById(`rule-${rule}`);
        if (element) {
            element.classList.toggle('valid', rules[rule]);
        }
    });

    return Object.values(rules).every(Boolean);
}

function validatePasswordMatch() {
    const password = document.getElementById('registerPassword').value;
    const confirm = document.getElementById('registerConfirmPassword').value;
    const matchElement = document.getElementById('passwordMatch');

    if (!password || !confirm) {
        matchElement.textContent = '';
        matchElement.classList.remove('valid', 'invalid');
        return false;
    }

    const isValid = password === confirm;
    matchElement.textContent = isValid ? 'Пароли совпадают' : 'Пароли не совпадают';
    matchElement.classList.toggle('valid', isValid);
    matchElement.classList.toggle('invalid', !isValid);

    return isValid;
}

// Работа с API
async function makeRequest(url, options = {}, requireCaptcha = false) {
    showLoader(true);

    try {
        const response = await fetch(`${API_BASE_URL}${url}`, {
            headers: {
                'Content-Type': 'application/json',
                ...options.headers
            },
            ...options
        });

        if (response.status === 403) {
            const errorData = await response.json();
            if (errorData.error && errorData.captcha) {
                // Показываем капчу
                pendingRequest = { url, options };
                showCaptchaModal(errorData);
                showLoader(false);
                return null;
            }
        }

        showLoader(false);

        if (!response.ok) {
            const error = await response.json();
            throw new Error(error.message || 'Ошибка сервера');
        }

        return response.status === 204 ? null : await response.json();
    } catch (error) {
        showLoader(false);
        showAlert(error.message);
        throw error;
    }
}

// Капча
function showCaptchaModal(captchaData) {
    currentRequestId = captchaData.requestId;
    document.getElementById('captchaImg').src = `data:image/png;base64,${captchaData.captcha}`;
    document.getElementById('captchaModal').classList.add('active');
    document.getElementById('captchaInput').value = '';
    document.getElementById('captchaError').textContent = '';
}

function closeCaptchaModal() {
    document.getElementById('captchaModal').classList.remove('active');
    currentRequestId = null;
    pendingRequest = null;
}

async function submitCaptcha() {
    const code = document.getElementById('captchaInput').value.trim();

    if (!code) {
        document.getElementById('captchaError').textContent = 'Введите код с картинки';
        return;
    }

    showLoader(true);

    try {
        const response = await fetch(`${API_BASE_URL}/Captcha/VerifyCaptcha?requestId=${currentRequestId}&code=${code}`, {
            method: 'POST'
        });

        if (response.status == 403) {
            switchTab('auth');
            throw new Error('Неверный код капчи');
        }
        if (response.status == 401) {
            switchTab('auth');
            throw new Error('Пользователь с такими данными не найден');
        }
        if (!response.ok) {
            switchTab('auth');
            throw new Error('Неверный код капчи');
        }
        // Выполняем отложенный запрос
        if (pendingRequest) {
            const result = await makeRequest(pendingRequest.url, pendingRequest.options, false);
            closeCaptchaModal();
            return result;
        }
    } catch (error) {
        showLoader(false);
        document.getElementById('captchaError').textContent = error.message;
    }
}

// Авторизация
async function handleAuthStep1(e) {
    e.preventDefault();

    const email = document.getElementById('authEmail').value;
    const password = document.getElementById('authPassword').value;

    state.currentEmail = email;

    try {
        await makeRequest('/Authentication/StartAuthenticationAsync', {
            method: 'POST',
            body: JSON.stringify({ mail: email, password })
        }, true);

        showStep('auth', 2);
    } catch (error) {
        // Ошибка уже обработана в makeRequest
    }
}

async function handleAuthStep2(e) {
    e.preventDefault();

    const code = document.getElementById('authCode').value;

    try {
        const result = await makeRequest(`/Authentication/EndAuthenticationAsync?mail=${state.currentEmail}&verifyCode=${code}`, {
            method: 'POST',
            body: JSON.stringify()
        });

        if (result) {
            state.refreshToken = result.refreshToken;
            state.isAuthenticated = true;
            state.userEmail = state.currentEmail;

            saveTokens(result);
            updateAuthStatus();
            showAlert('Авторизация успешна!', 'success');
            switchTab('tokens');
        }
    } catch (error) {
        // Ошибка уже обработана
    }
}

// Регистрация
async function handleRegisterStep1(e) {
    e.preventDefault();

    const email = document.getElementById('registerEmail').value;
    const password = document.getElementById('registerPassword').value;
    const confirmPassword = document.getElementById('registerConfirmPassword').value;

    // Валидация
    if (!validatePassword()) {
        showAlert('Пароль не соответствует требованиям безопасности');
        return;
    }

    if (!validatePasswordMatch()) {
        showAlert('Пароли не совпадают');
        return;
    }

    if (password !== confirmPassword) {
        showAlert('Пароли не совпадают');
        return;
    }

    state.currentEmail = email;

    try {
        await makeRequest('/Registration/StartRegistrationAsync', {
            method: 'POST',
            body: JSON.stringify({ mail: email, password })
        }, true);

        showStep('register', 2);
    } catch (error) {
        // Ошибка уже обработана
    }
}

async function handleRegisterStep2(e) {
    e.preventDefault();

    const code = document.getElementById('registerCode').value;

    try {
        await makeRequest(`/Registration/EndRegistrationAsync?code=${code}`, {
            method: 'POST',
            body: JSON.stringify()
        });

        showAlert('Регистрация успешно завершена!', 'success');
        switchTab('auth');
    } catch (error) {
        // Ошибка уже обработана
    }
}

// Восстановление пароля
async function handleRecoverStep1(e) {
    e.preventDefault();

    const email = document.getElementById('recoverEmail').value;
    state.currentEmail = email;

    try {
        await makeRequest(`/Recover/StartRecoverPasswordAsync?mail=${email}`, {
            method: 'POST',
            body: JSON.stringify()
        }, true);

        showStep('recover', 2);
    } catch (error) {
        // Ошибка уже обработана
    }
}

async function handleRecoverStep2(e) {
    e.preventDefault();

    const code = document.getElementById('recoverCode').value;
    const newPassword = document.getElementById('newPassword').value;
    const confirmPassword = document.getElementById('confirmNewPassword').value;

    if (newPassword !== confirmPassword) {
        showAlert('Пароли не совпадают');
        return;
    }

    // Проверка сложности пароля
    const passwordRegex = /^(?=.*[a-z])(?=.*[A-Z])(?=.*[!@#$%^&*()_+\-=\[\]{};':"\\|,.<>\/?]).{8,}$/;
    if (!passwordRegex.test(newPassword)) {
        showAlert('Новый пароль не соответствует требованиям безопасности');
        return;
    }

    try {
        await makeRequest(`/Recover/EndRecoverPasswordAsync?mail=${state.currentEmail}&code=${code}&newPassword=${newPassword}`, {
            method: 'POST',
            body: JSON.stringify()
        });

        showAlert('Пароль успешно изменен!', 'success');
        switchTab('auth');
    } catch (error) {
        // Ошибка уже обработана
    }
}

// Управление токенами
function saveTokens(tokenPair) {
    if (tokenPair.refreshToken) {
        localStorage.setItem('refreshToken', tokenPair.refreshToken);
        localStorage.setItem('accessToken', tokenPair.accessToken);
        state.refreshToken = tokenPair.refreshToken;
        loadTokens();
    }
}

































const state = {
    isAuthenticated: false,
    userEmail: null,
    refreshToken: null,
    currentEmail: null,
    hasPin: false
};

// Инициализация приложения
document.addEventListener('DOMContentLoaded', () => {
    initEventListeners();
    loadUserData();
    updateAuthStatus();
    setupPinInputs();
});

function loadUserData() {
    const userData = JSON.parse(localStorage.getItem('userData'));
    if (userData) {
        state.userEmail = userData.email;
        state.refreshToken = userData.refreshToken;
        state.hasPin = userData.hasPin || false;
        state.isAuthenticated = false; // Требуется пин-код для доступа

        if (state.hasPin && state.refreshToken) {
            // Автоматически показываем запрос пин-кода при загрузке
            setTimeout(() => showPinVerifyModal(), 500);
        }
    }
}

function saveUserData() {
    const userData = {
        email: state.userEmail,
        refreshToken: state.refreshToken,
        hasPin: state.hasPin
    };
    localStorage.setItem('userData', JSON.stringify(userData));
}

function clearUserData() {
    localStorage.removeItem('userData');
    state.userEmail = null;
    state.refreshToken = null;
    state.hasPin = false;
    state.isAuthenticated = false;
}

function initEventListeners() {
    // Существующие обработчики...
    // Переключение табов
    document.querySelectorAll('.tab').forEach(tab => {
        tab.addEventListener('click', () => switchTab(tab.dataset.tab));
    });

    // Переключение видимости пароля
    document.querySelectorAll('.toggle-password').forEach(button => {
        button.addEventListener('click', (e) => {
            const targetId = e.target.closest('button').dataset.target;
            const input = document.getElementById(targetId);
            const icon = e.target.closest('button').querySelector('i');

            if (input.type === 'password') {
                input.type = 'text';
                icon.className = 'fas fa-eye-slash';
            } else {
                input.type = 'password';
                icon.className = 'fas fa-eye';
            }
        });
    });

    // Валидация пароля при регистрации
    document.getElementById('registerPassword').addEventListener('input', validatePassword);
    document.getElementById('registerConfirmPassword').addEventListener('input', validatePasswordMatch);

    // Формы
    document.getElementById('authForm').addEventListener('submit', handleAuthStep1);
    document.getElementById('authCodeForm').addEventListener('submit', handleAuthStep2);
    document.getElementById('backToAuth').addEventListener('click', () => showStep('auth', 1));

    document.getElementById('registerForm').addEventListener('submit', handleRegisterStep1);
    document.getElementById('registerCodeForm').addEventListener('submit', handleRegisterStep2);
    document.getElementById('backToRegister').addEventListener('click', () => showStep('register', 1));

    document.getElementById('recoverForm').addEventListener('submit', handleRecoverStep1);
    document.getElementById('recoverCodeForm').addEventListener('submit', handleRecoverStep2);
    document.getElementById('backToRecover').addEventListener('click', () => showStep('recover', 1));

    // Управление токенами
    document.getElementById('refreshAccessToken').addEventListener('click', () => {
        if (state.hasPin) {
            showPinVerifyModal();
        } else {
            refreshAccessToken();
        }
    });
    document.getElementById('clearTokens').addEventListener('click', clearTokens);

    // Капча
    document.querySelector('.close-modal').addEventListener('click', closeCaptchaModal);
    document.getElementById('submitCaptcha').addEventListener('click', submitCaptcha);

    // Пин-код: предложение
    document.getElementById('acceptPin').addEventListener('click', showPinSetupModal);
    document.getElementById('declinePin').addEventListener('click', declinePin);

    // Пин-код: установка
    document.getElementById('savePin').addEventListener('click', savePin);

    // Пин-код: верификация
    document.getElementById('verifyPin').addEventListener('click', verifyPin);
    document.getElementById('cancelPin').addEventListener('click', cancelPin);

    // Отображение токена
    document.getElementById('copyToken').addEventListener('click', copyToken);
    document.getElementById('closeTokenModal').addEventListener('click', closeTokenModal);

    // Закрытие модальных окон
    document.querySelectorAll('.close-modal').forEach(btn => {
        btn.addEventListener('click', function () {
            const modalId = this.dataset.modal;
            closeModal(modalId);
        });
    });
}

function setupPinInputs() {
    // Настройка автоперехода между полями пин-кода
    const setupPinAutoAdvance = (inputs) => {
        inputs.forEach((input, index) => {
            input.addEventListener('input', (e) => {
                if (e.target.value.length === 1 && index < inputs.length - 1) {
                    inputs[index + 1].focus();
                }
            });

            input.addEventListener('keydown', (e) => {
                if (e.key === 'Backspace' && !e.target.value && index > 0) {
                    inputs[index - 1].focus();
                }
            });
        });
    };

    // Для установки пин-кода
    const pinSetupInputs = [
        document.getElementById('pinInput1'),
        document.getElementById('pinInput2'),
        document.getElementById('pinInput3'),
        document.getElementById('pinInput4')
    ];

    // Для верификации пин-кода
    const pinVerifyInputs = [
        document.getElementById('pinVerify1'),
        document.getElementById('pinVerify2'),
        document.getElementById('pinVerify3'),
        document.getElementById('pinVerify4')
    ];

    setupPinAutoAdvance(pinSetupInputs);
    setupPinAutoAdvance(pinVerifyInputs);
}

// Модальные окна
function showModal(modalId) {
    document.getElementById(modalId).classList.add('active');
}

function closeModal(modalId) {
    document.getElementById(modalId).classList.remove('active');
}

function showPinOfferModal() {
    showModal('pinOfferModal');
}

function showPinSetupModal() {
    closeModal('pinOfferModal');
    // Сброс полей
    ['pinInput1', 'pinInput2', 'pinInput3', 'pinInput4'].forEach(id => {
        document.getElementById(id).value = '';
    });
    document.getElementById('pinError').textContent = '';
    document.getElementById('pinInput1').focus();
    showModal('pinSetupModal');
}

function showPinVerifyModal() {
    // Сброс полей
    ['pinVerify1', 'pinVerify2', 'pinVerify3', 'pinVerify4'].forEach(id => {
        document.getElementById(id).value = '';
    });
    document.getElementById('pinVerifyError').textContent = '';
    document.getElementById('pinVerify1').focus();
    showModal('pinVerifyModal');
}

function showTokenDisplayModal(token) {
    document.getElementById('tokenDisplay').textContent = token;
    showModal('tokenDisplayModal');
}

// Обработка пин-кода
function declinePin() {
    closeModal('pinOfferModal');
    // Показываем токен пользователю
    showTokenDisplayModal(currentTokenPair.refreshToken);

    // Не сохраняем refresh токен
    state.refreshToken = null;
    state.hasPin = false;
    clearUserData();

    // Показываем сообщение
    showAlert('Refresh token не сохранен. Скопируйте и сохраните его в надежном месте.', 'warning');
}

function savePin() {
    const pin1 = document.getElementById('pinInput1').value;
    const pin2 = document.getElementById('pinInput2').value;
    const pin3 = document.getElementById('pinInput3').value;
    const pin4 = document.getElementById('pinInput4').value;

    const pinCode = pin1 + pin2 + pin3 + pin4;

    // Проверка пин-кода
    if (pinCode.length !== 4 || !/^\d{4}$/.test(pinCode)) {
        document.getElementById('pinError').textContent = 'Пин-код должен состоять из 4 цифр';
        return;
    }

    // Сохраняем пин-код (в реальном приложении нужно хэшировать!)
    state.hasPin = true;
    state.refreshToken = currentTokenPair.refreshToken;

    // Сохраняем пин-код локально (для демо - не безопасно!)
    localStorage.setItem(`pin_${state.userEmail}`, pinCode);
    saveUserData();

    closeModal('pinSetupModal');
    showAlert('Пин-код успешно установлен!', 'success');
    updateAuthStatus();
}

function verifyPin() {
    const pin1 = document.getElementById('pinVerify1').value;
    const pin2 = document.getElementById('pinVerify2').value;
    const pin3 = document.getElementById('pinVerify3').value;
    const pin4 = document.getElementById('pinVerify4').value;

    const enteredPin = pin1 + pin2 + pin3 + pin4;

    // Проверка пин-кода
    if (enteredPin.length !== 4 || !/^\d{4}$/.test(enteredPin)) {
        document.getElementById('pinVerifyError').textContent = 'Пин-код должен состоять из 4 цифр';
        return;
    }

    // Получаем сохраненный пин-код
    const savedPin = localStorage.getItem(`pin_${state.userEmail}`);

    if (savedPin === enteredPin) {
        // Пин-код верный
        closeModal('pinVerifyModal');
        state.isAuthenticated = true;
        updateAuthStatus();

        // Если есть refresh токен, обновляем access токен
        if (state.refreshToken) {
            refreshAccessToken();
        }
    } else {
        document.getElementById('pinVerifyError').textContent = 'Неверный пин-код';
        // Анимация ошибки
        const inputs = ['pinVerify1', 'pinVerify2', 'pinVerify3', 'pinVerify4'];
        inputs.forEach(id => {
            const input = document.getElementById(id);
            input.classList.add('error');
            setTimeout(() => input.classList.remove('error'), 500);
        });
    }
}

function cancelPin() {
    closeModal('pinVerifyModal');
    clearUserData();
    clearTokens();
    switchTab('auth');
    showAlert('Для входа потребуется полная аутентификация', 'info');
}

function copyToken() {
    const token = document.getElementById('tokenDisplay').textContent;
    navigator.clipboard.writeText(token).then(() => {
        showAlert('Токен скопирован в буфер обмена', 'success');
    });
}

function closeTokenModal() {
    closeModal('tokenDisplayModal');
}

// Обновленная авторизация
async function handleAuthStep2(e) {
    e.preventDefault();

    const code = document.getElementById('authCode').value;

    try {
        const result = await makeRequest(`/Authentication/EndAuthenticationAsync?mail=${state.currentEmail}&verifyCode=${code}`, {
            method: 'POST',
            body: JSON.stringify()
        });

        if (result) {
            currentTokenPair = result;
            state.userEmail = state.currentEmail;
            state.isAuthenticated = true;

            // Показываем предложение установить пин-код
            showPinOfferModal();

            showAlert('Авторизация успешна!', 'success');
        }
    } catch (error) {
        // Ошибка уже обработана
    }
}

// Обновленное управление токенами
async function refreshAccessToken() {
    if (!state.refreshToken) {
        showAlert('Refresh токен не найден', 'error');
        return;
    }

    try {
        const result = await makeRequest(`/Authentication/GetNewTokenPairAsync`, {
            method: 'GET',
            headers: {
                'Authorization': `Bearer ${state.refreshToken}`
            }
        });

        if (result) {
            // Обновляем access токен
            localStorage.setItem('accessToken', result.accessToken);

            document.getElementById('tokenResult').textContent = 'Access токен успешно обновлен!';
            document.getElementById('tokenResult').className = 'token-result success';
            setTimeout(() => {
                document.getElementById('tokenResult').className = 'token-result';
            }, 3000);

            // Если у нас еще нет refresh токена (например, после ввода пин-кода)
            if (!state.refreshToken) {
                state.refreshToken = result.refreshToken;
                saveUserData();
            }
        }
    } catch (error) {
        document.getElementById('tokenResult').textContent = `Ошибка: ${error.message}`;
        document.getElementById('tokenResult').className = 'token-result error';

        // Если ошибка связана с токеном, очищаем данные
        if (error.message.includes('Invalid token') || error.message.includes('Forbidden')) {
            clearUserData();
            state.isAuthenticated = false;
            updateAuthStatus();
        }
    }
}

function clearTokens() {
    clearUserData();
    localStorage.removeItem('accessToken');

    // Очищаем все пин-коды для этого пользователя
    if (state.userEmail) {
        localStorage.removeItem(`pin_${state.userEmail}`);
    }

    loadTokens();
    updateAuthStatus();

    document.getElementById('tokenResult').textContent = 'Токены и пин-код успешно удалены';
    document.getElementById('tokenResult').className = 'token-result success';
    setTimeout(() => {
        document.getElementById('tokenResult').className = 'token-result';
    }, 3000);
}

function loadTokens() {
    const refreshToken = state.refreshToken;
    const tokenDisplay = document.getElementById('refreshTokenDisplay');

    if (refreshToken) {
        // Показываем только начало и конец токена для безопасности
        const shortToken = refreshToken.length > 30
            ? `${refreshToken.substring(0, 15)}...${refreshToken.substring(refreshToken.length - 15)}`
            : refreshToken;

        tokenDisplay.textContent = shortToken;
        tokenDisplay.title = refreshToken;

        // Добавляем информацию о пин-коде
        if (state.hasPin) {
            tokenDisplay.innerHTML += '<br><span class="pin-status has-pin">Защищено пин-кодом</span>';
        }
    } else {
        tokenDisplay.textContent = 'Токен не сохранен';
        tokenDisplay.title = '';
    }
}

function updateAuthStatus() {
    const authStatus = document.getElementById('authStatus');

    if (state.isAuthenticated && state.userEmail) {
        let statusText = `Авторизован: ${state.userEmail}`;
        if (state.hasPin) {
            statusText += ' (защищено пин-кодом)';
        }
        authStatus.textContent = statusText;
        authStatus.style.background = 'rgba(40, 167, 69, 0.2)';
    } else if (state.refreshToken) {
        authStatus.textContent = 'Имеется сохраненный токен (требуется пин-код)';
        authStatus.style.background = 'rgba(255, 193, 7, 0.2)';
    } else {
        authStatus.textContent = 'Не авторизован';
        authStatus.style.background = 'rgba(255, 255, 255, 0.2)';
    }

    loadTokens();
}