import { applyMiddleware, combineReducers, compose, createStore } from 'redux';
import thunk from 'redux-thunk';
import { routerReducer, routerMiddleware } from 'react-router-redux';
import * as Counter from './Counter';
import * as WeatherForecasts from './WeatherForecasts';
import * as SignalR from '@aspnet/signalr';
import * as CL from './ConsoleLog';

export default function configureStore(history, initialState) {
      const reducers = {
        counter: Counter.reducer,
        weatherForecasts: WeatherForecasts.reducer,
        consoleLog: CL.reducer
      };

      const middleware = [
        thunk,
        routerMiddleware(history)
      ];

      // In development, use the browser's Redux dev tools extension if installed
      const enhancers = [];
      const isDevelopment = process.env.NODE_ENV === 'development';
      if (isDevelopment && typeof window !== 'undefined' && window.devToolsExtension) {
        enhancers.push(window.devToolsExtension());
      }

      const rootReducer = combineReducers({
        ...reducers,
        routing: routerReducer
        });

        const connection = new SignalR.HubConnectionBuilder()
            .withUrl(window.location.protocol + '//' + window.location.host + '/console')
            .build();

        let store = createStore(
            rootReducer,
            initialState,
            compose(applyMiddleware(...middleware), ...enhancers)
        );

        connection.on('get_train_log', logging => {
            store.dispatch(({ type: CL.log2TrainConsoleType, logging }));
        });

        connection.on('get_predict_log', logging => {
            store.dispatch(({ type: CL.log2PredictConsoleType, logging }));
        });

        Promise.resolve(connection.start());

        return store;
}
