$(document).ready(function () {
    checkFirstVisit();
});

function checkFirstVisit() {
    const firstVisit = sessionStorage.getItem('firstVisit');

    if (!firstVisit) {
        sessionStorage.setItem('firstVisit', 'false');
        initializeServiceWorkerAndPushManager();
    }
}

function initializeServiceWorkerAndPushManager() {
    if (isPushNotificationSupported()) {
        sessionStorage.setItem("support", "true");

        navigator.serviceWorker.register('sw.js')
            .then(registration => {
                sessionStorage.setItem("sw", "registered");
                handlePushSubscription(registration);
            })
            .catch(error => {
                sessionStorage.setItem("sw", "not registered");
            });
    } else {
        sessionStorage.setItem("support", "false");
    }
}

function isPushNotificationSupported() {
    return 'serviceWorker' in navigator && 'PushManager' in window && 'Notification' in window;
}

function handlePushSubscription(registration) {
    registration.pushManager.getSubscription()
        .then(subscription => {
            if (subscription) {
                sessionStorage.setItem("subscribed", "true");
                updateSubscriptionOnServer(subscription);
            } else {
                sessionStorage.setItem("subscribed", "false");
            }
        })
        .catch(error => {
        });
}

function updateSubscriptionOnServer(subscription) {
    $.ajax({
        url: '/Home/CreateOrUpdatePushSubscription',
        type: 'POST',
        contentType: 'application/json',
        headers: {
            'RequestVerificationToken': $('input[name=__RequestVerificationToken]').val()
        },
        data: JSON.stringify(subscription),
        success: response => {

        },
        error: (xhr, status, error) => {

        }
    });
}