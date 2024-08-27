function urlB64ToUint8Array(base64String) {
    const padding = '='.repeat((4 - base64String.length % 4) % 4);
    const base64 = (base64String + padding)
        .replace(/-/g, '+')
        .replace(/_/g, '/');

    const rawData = window.atob(base64);
    return new Uint8Array([...rawData].map(char => char.charCodeAt(0)));
}

function ajaxRequest(url, type, data, successCallback) {
    $.ajax({
        url: url,
        type: type,
        contentType: 'application/json',
        headers: {
            'RequestVerificationToken': $('input[name=__RequestVerificationToken]').val()
        },
        data: data ? JSON.stringify(data) : undefined,
        success: successCallback,
        error: function (error) {
            initializeUI();

        }
    });
}

function updateSubscriptionOnServer(subscription) {
    ajaxRequest('/Account/CreateOrUpdatePushSubscription', 'POST', subscription, function () {
        sessionStorage.setItem("subscribed", "true");
        initializeUI();
    });
}

function deleteSubscriptionOnServer() {
    ajaxRequest('/Account/DeletePushSubscription', 'DELETE', null, function () {
        sessionStorage.setItem("subscribed", "false");
        initializeUI();
    });
}

function subscribeUser() {
    const applicationServerKey = urlB64ToUint8Array("@Model.PublicVapidKey");
    navigator.serviceWorker.getRegistration().then(function (registration) {
        registration.pushManager.subscribe({
            userVisibleOnly: true,
            applicationServerKey: applicationServerKey
        }).then(updateSubscriptionOnServer)
            .catch(function (err) {

            });
    });
}

function unsubscribeUser() {
    navigator.serviceWorker.getRegistration().then(function (registration) {
        registration.pushManager.getSubscription().then(function (subscription) {
            if (subscription) {
                subscription.unsubscribe().then(deleteSubscriptionOnServer)
                    .catch(function (err) {

                    });
            }
        }).catch(function (err) {

        });
    });
}

function initializeUI() {
    const pushButton = $('#notifications-btn');
    const isSubscribed = sessionStorage.getItem("subscribed");

    if (Notification.permission === 'denied') {
        pushButton.text('Push Messaging Blocked.').prop('disabled', true);
    } else {
        pushButton.text(isSubscribed === 'true' ? 'Disable' : 'Enable');
    }
}

$(document).ready(function () {
    const isSupported = sessionStorage.getItem("support");

    $('#notifications-btn').on('click', function () {
        if (isSupported === 'true') {
            const isSubscribed = sessionStorage.getItem("subscribed");
            if (isSubscribed === 'false') {
                $('#notifications-btn').text('Loading...');
                subscribeUser();
            } else if (isSubscribed === 'true') {
                $('#notifications-btn').text('Loading...');
                unsubscribeUser();
            }
        }
    });

    initializeUI();
});