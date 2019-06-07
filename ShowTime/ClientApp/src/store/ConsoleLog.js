export const log2ConsoleType = 'LOG_2_CONSOLE';
const logList = ["Начинаю лог"];

export const actionCreators = {
    logServer: logging => ({ type: log2ConsoleType, logging })
};

export const reducer = (state, action) => {
    state = state || { logging: logList, len: logList.length };

    if (action.type === log2ConsoleType) {
        logList.push(action.logging);
        state = { logging: logList, len: logList.length };
    }

    return state;
};