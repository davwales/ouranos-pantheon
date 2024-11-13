interface Setting<T> {
    id: string,
    module: string,
    type: string,
    key: string,
    name: string,
    value: T
};

export default Setting;