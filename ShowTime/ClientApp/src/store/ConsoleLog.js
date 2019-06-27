export const log2TrainConsoleType = 'LOG_2_TRAIN_CONSOLE';
export const log2PredictConsoleType = 'LOG_2_PREDICT_CONSOLE';
export const log2TransferConsoleType = 'LOG_2_TRANSFER_CONSOLE';
let logList = ["Начинаю лог"];
let currentLog = log2TrainConsoleType;

function makeLog(action, state) {
    if (currentLog !== action.type) {
        logList = ["Начинаю лог"];
        currentLog = action.type;
    }
    logList.push(action.logging);
    return { logging: logList, len: logList.length };
}

export const actionCreators = {
    logTrainServer: logging => ({ type: log2TrainConsoleType, logging }),
    logPredictServer: logging => ({ type: log2TrainConsoleType, logging }),
    logTransferServer: logging => ({ type: log2TransferConsoleType, logging })
};

export const reducer = (state, action) => {
    state = state || { logging: logList, len: logList.length };

    return makeLog(action, state);
};