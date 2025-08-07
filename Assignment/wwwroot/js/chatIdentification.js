function createUUID() {
    return 'xxxxxxxx-xxxx-4xxx-yxxx-xxxxxxxxxxxx'.replace(/[xy]/g, function (c) {
        var r = Math.random() * 16 | 0,
            v = c == 'x' ? r : (r & 0x3 | 0x8);
        return v.toString(16);
    });
}

function ensureChatId() {
    let chatId = localStorage.getItem('ChatId');

    if (!chatId) {
        const newChatId = createUUID();
        localStorage.setItem('ChatId', newChatId);
        return newChatId;
    } else {
        return chatId;
    }
}

const currentChatId = ensureChatId();