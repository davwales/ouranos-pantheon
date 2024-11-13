import { CodegenConfig } from '@graphql-codegen/cli';

const config: CodegenConfig = {
    schema: 'http://192.168.4.89:8300/graphql',
    documents: ['src/**/*.tsx'],
    ignoreNoDocuments: true,
    generates: {
        './src/gql/': {
            preset: 'client',
            plugins: [],
        },
    },
};

export default config;