export const log2ConsoleType = 'LOG_2_CONSOLE';
const initialState = { logging: null };

export const actionCreators = {
    logServer: logging => ({ type: log2ConsoleType, logging })
};

export const reducer = (state, action) => {
    state = state || initialState;

    if (action.type === log2ConsoleType) {
        state = { ...state, logging: action.logging };
    }

    return state;
};
