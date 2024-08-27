'use strict';

//listen to any push event
self.addEventListener('push', function (event) {
    if (event.data) {
        var payload = JSON.parse(event.data.text()); // Correctly parse the push data

        const title = payload.Title ? payload.Title : 'Document Request';

        // Define options for the notification
        const options = {
            body: payload.Body ? payload.Body : 'New changes are available',
            icon: '/favicon.ico',
            badge: '/favicon.ico',
            data: {
                tag: payload.Tag, // Correct the typo in tag
                clickActionUrl: self.location.origin + (payload.ClickAction ? payload.ClickAction : '/'),
            },
            actions: [
                {
                    action: 'close',
                    title: 'Close'
                }
            ],
            tag: payload.Tag
        };

        // Wait until notification is displayed
        event.waitUntil(self.registration.showNotification(title, options));
    } else {
        // Optionally handle the case where the payload is not valid
        console.error('Invalid push data');
    }
});


//listen to any notification click
self.addEventListener('notificationclick', function (event) {
    var notification = event.notification
    var action = event.action
    if (action === 'close') {
        notification.close();
    }
    else {
        notification.close();
        self.clients.openWindow(event.notification.data.clickActionUrl);
    }
});