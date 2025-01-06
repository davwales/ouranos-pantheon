// CharacterForm.tsx
"use client";

import React, { useState } from 'react';
import {
    Box,
    Button,
    TextField,
    Typography,
    IconButton,
} from '@mui/material';
import { Add, Remove } from '@mui/icons-material';
import { CharacterDetail } from '@/gql/graphql';

export interface CharacterInput {
    name: string;
    age: number;
    details: CharacterDetail[];
}

interface CharacterFormProps {
    initialValues?: CharacterInput;
    onSubmit: (input: CharacterInput) => Promise<void>;
    loading?: boolean;
}

export default function CharacterForm(props: CharacterFormProps) {
    const [name, setName] = useState(props.initialValues?.name || '');
    const [age, setAge] = useState<number | ''>(props.initialValues?.age || '');
    const [details, setDetails] = useState<CharacterDetail[]>(
        props.initialValues?.details || []
    );

    const handleChange =
        (setter: React.Dispatch<React.SetStateAction<any>>) =>
            (event: React.ChangeEvent<HTMLInputElement>) => {
                setter(event.target.value);
            };

    const handleDetailsChange = (
        index: number,
        field: 'key' | 'value',
        event: React.ChangeEvent<HTMLInputElement | HTMLTextAreaElement>
    ) => {
        const newDetails = [...details];
        newDetails[index][field] = event.target.value;
        setDetails(newDetails);
    };

    const handleAddDetail = () => {
        setDetails([...details, { key: '', value: '' }]);
    };

    const handleRemoveDetail = (index: number) => {
        const newDetails = details.filter((_, i) => i !== index);
        setDetails(newDetails);
    };

    const handleSubmit = async (event: React.FormEvent) => {
        event.preventDefault();
        const input: CharacterInput = {
            name,
            age: age === '' ? 0 : Number(age),
            details,
        };
        await props.onSubmit(input);
    };

    return (
        <form onSubmit={handleSubmit}>
            <TextField
                label="Name"
                value={name}
                onChange={handleChange(setName)}
                required
                fullWidth
                margin="normal"
            />
            <TextField
                label="Age"
                type="number"
                value={age}
                onChange={handleChange(setAge)}
                fullWidth
                margin="normal"
            />
            <Typography variant="h6" gutterBottom sx={{ mb: "1rem" }}>
                Details
            </Typography>
            {details.map((detail, index) => (
                <Box
                    key={index}
                    sx={{ display: 'flex', alignItems: 'center', mb: "1rem" }}
                >
                    <TextField
                        label="Key"
                        value={detail.key}
                        onChange={(e) => handleDetailsChange(index, 'key', e)}
                        required
                        sx={{ flex: 1, mr: 1 }}
                    />
                    <TextField
                        label="Value"
                        value={detail.value}
                        onChange={(e) => handleDetailsChange(index, 'value', e)}
                        required
                        sx={{ flex: 1, mr: 1 }}
                    />
                    <IconButton onClick={() => handleRemoveDetail(index)}>
                        <Remove />
                    </IconButton>
                </Box>
            ))}
            <Button
                variant="outlined"
                onClick={handleAddDetail}
                startIcon={<Add />}
                sx={{ mb: 2 }}
            >
                Add Detail
            </Button>
            <Box sx={{ mt: 2 }}>
                <Button
                    type="submit"
                    variant="contained"
                    disabled={props.loading}
                >
                    {props.loading ? 'Submitting...' : 'Submit'}
                </Button>
            </Box>
        </form>
    );
}
