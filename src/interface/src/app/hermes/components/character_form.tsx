"use client";

import Typography from '@/app/components/core/data-display/typography';
import AddIcon from '@/app/components/core/icons/add_icon';
import RemoveIcon from '@/app/components/core/icons/remove_icon';
import Button from '@/app/components/core/inputs/button';
import IconButton from '@/app/components/core/inputs/icon_button';
import NumberField from '@/app/components/core/inputs/number_field';
import TextField from '@/app/components/core/inputs/text_field';
import Box from '@/app/components/core/layout/box';
import FormBox from '@/app/components/core/layout/form_box';
import { CharacterDetail } from '@/gql/graphql';
import React, { useState } from 'react';

export interface CharacterInput {
    id?: string;
    name: string;
    age: number;
    details: CharacterDetail[];
}

interface CharacterFormProps {
    initialValues?: CharacterInput;
    onSubmit: (input: CharacterInput) => void;
    loading?: boolean;
}

export default function CharacterForm(props: CharacterFormProps) {
    const [name, setName] = useState(props.initialValues?.name || '');
    const [age, setAge] = useState<number | undefined>(props.initialValues?.age);
    const [details, setDetails] = useState<CharacterDetail[]>(props.initialValues?.details || []);

    const handleNameChange = (x: string) => {
        if (!x) return;
        setName(x);
    };

    const handleAgeChange = (x: number) => {
        if (x <= 0) return;
        setAge(x);
    }

    const handleDetailsChange = (
        index: number,
        field: 'key' | 'value',
        value: string
    ) => {
        const newDetails = [...details];
        newDetails[index][field] = value;
        setDetails(newDetails);
    };

    const handleAddDetail = () => {
        setDetails([...details, { key: '', value: '' }]);
    };

    const handleRemoveDetail = (index: number) => {
        const newDetails = details.filter((_, i) => i !== index);
        setDetails(newDetails);
    };

    const handleSubmit = (event: React.FormEvent) => {
        event.preventDefault();
        const input: CharacterInput = {
            id: props.initialValues?.id,
            name,
            age: age ?? 0,
            details,
        };
        props.onSubmit(input);
    };

    return (
        <FormBox onSubmit={handleSubmit}>
            <TextField
                label="Name"
                value={name}
                onChange={handleNameChange}
                required
                fullWidth
                margin="normal"
            />

            <NumberField
                label="Age"
                value={age}
                onChange={handleAgeChange}
                fullWidth
                margin="normal"
            />

            <Typography variant="h6" gutterBottom styling={{ mb: "medium" }}>
                Details
            </Typography>

            {details.map((detail, index) => (
                <Box
                    key={index}
                    styling={{ display: 'flex', alignItems: 'center', mb: "medium" }}
                >
                    <TextField
                        label="Key"
                        value={detail.key}
                        onChange={(x) => handleDetailsChange(index, 'key', x)}
                        required
                        styling={{ flex: 1, mr: "medium" }}
                    />
                    <TextField
                        label="Value"
                        value={detail.value}
                        onChange={(x) => handleDetailsChange(index, 'value', x)}
                        required
                        styling={{ flex: 1, mr: "medium" }}
                    />
                    <IconButton onClick={() => handleRemoveDetail(index)}>
                        <RemoveIcon />
                    </IconButton>
                </Box>
            ))}

            <Button
                variant="outlined"
                onClick={handleAddDetail}
                startIcon={<AddIcon />}
                styling={{ mb: 'small' }}
            >
                Add Detail
            </Button>

            <Box>
                <Button
                    variant="contained"
                    disabled={props.loading}
                    submit
                >
                    {props.loading ? 'Submitting...' : 'Submit'}
                </Button>
            </Box>
        </FormBox>
    );
}
