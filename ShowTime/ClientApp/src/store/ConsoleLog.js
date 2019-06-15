export const log2TrainConsoleType = 'LOG_2_TRAIN_CONSOLE';
export const log2PredictConsoleType = 'LOG_2_PREDICT_CONSOLE';
let logList = ["Начинаю лог"];
let isTrain = true;

export const actionCreators = {
    logTrainServer: logging => ({ type: log2TrainConsoleType, logging }),
    logPredictServer: logging => ({ type: log2TrainConsoleType, logging })
};

export const reducer = (state, action) => {
    state = state || { logging: logList, len: logList.length };

    if (action.type === log2TrainConsoleType) {
        if (!isTrain) {
            logList = ["Начинаю лог"];
            isTrain = true;
        }
        logList.push(action.logging);
        state = { logging: logList, len: logList.length };
    }

    if (action.type === log2PredictConsoleType) {
        if (isTrain) {
            logList = ["Начинаю лог"];
            isTrain = false;
        }
        logList.push(action.logging);
        state = { logging: logList, len: logList.length };
    }

    return state;
};