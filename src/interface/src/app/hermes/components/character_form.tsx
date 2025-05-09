"use client";

import { Typography } from '@/app/components/typography';
import { Button } from '@/components/ui/button';
import { Input } from '@/components/ui/input';
import { Separator } from '@/components/ui/separator';
import { CharacterDetail } from '@/gql/graphql';
import React, { ChangeEvent, useMemo, useState } from 'react';

export interface CharacterInput {
    id?: string;
    name: string;
    age: number;
    details: CharacterDetail[];
}

export function CharacterForm({
    onSave,
    onDelete,
    submitText,
    initialValues,
    loading,
    ...props
}: React.ComponentProps<"form"> & {
    onSave?: (input: CharacterInput) => void;
    submitText?: string;
    onDelete?: () => void;
    initialValues?: CharacterInput;
    loading?: boolean;
}) {
    const [name, setName] = useState(initialValues?.name || '');
    const [age, setAge] = useState<number | undefined>(initialValues?.age);
    const [details, setDetails] = useState<CharacterDetail[]>(initialValues?.details || []);

    const handleNameChange = (event: ChangeEvent<HTMLInputElement>) => {
        setName(event.target.value);
    };

    const handleAgeChange = (event: ChangeEvent<HTMLInputElement>) => {
        const age = parseInt(event.target.value);
        if (age) {
            setAge(age);
        }
    }

    const handleDetailKeyChange = (index: number, event: ChangeEvent<HTMLInputElement>) => {
        const newDetails = [...details];
        newDetails[index].key = event.target.value;
        setDetails(newDetails);
    };

    const handleDetailValueChange = (index: number, event: ChangeEvent<HTMLInputElement>) => {
        const newDetails = [...details];
        newDetails[index].value = event.target.value;
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

        if (onSave) {
            const input: CharacterInput = {
                id: initialValues?.id,
                name,
                age: age ?? 0,
                details,
            };

            onSave(input);
        }
    };

    const isReadOnly = useMemo(() => !Boolean(onSave), [onSave]);

    return (
        <form {...props} onSubmit={handleSubmit}>
            <div className="grid grid-cols-1 md:grid-cols-2 gap-4 mt-4">
                <Typography variant="h4">Name</Typography>
                <Input
                    type="text"
                    readOnly={isReadOnly}
                    value={name}
                    onChange={handleNameChange}
                    className="w-full"
                />
            </div>

            <div className="grid grid-cols-1 md:grid-cols-2 gap-4 mt-4">
                <Typography variant="h4">Age</Typography>
                <Input
                    type="number"
                    readOnly={isReadOnly}
                    value={age}
                    onChange={handleAgeChange}
                    className="w-full"
                />
            </div>

            <Typography variant="h4" className="mt-4">Details</Typography>
            {details.length > 0 ? details.map((detail, index) => (
                <div key={index}>
                    <div className={`grid grid-cols-1 md:grid-cols-${isReadOnly ? '2' : '3'} gap-4 mt-4`}>
                        <Input
                            type="text"
                            readOnly={isReadOnly}
                            value={detail.key}
                            onChange={(e) => handleDetailKeyChange(index, e)}
                            className="w-full"
                        />

                        <Input
                            type="text"
                            readOnly={isReadOnly}
                            value={detail.value}
                            onChange={(e) => handleDetailValueChange(index, e)}
                            className="w-full"
                        />

                        {!isReadOnly && (
                            <Button type="button" variant="destructive" onClick={() => handleRemoveDetail(index)}>
                                Remove Detail
                            </Button>
                        )}
                    </div>
                    {index < details.length - 1 && <Separator className="mt-4" />}
                </div>
            )) : <Typography>Add details that describe your character!</Typography>}

            {!isReadOnly && (
                <div>
                    <Separator className="mt-4" />

                    <Button type="button" variant="outline" onClick={handleAddDetail} className="mt-4 w-full">
                        Add Detail
                    </Button>

                    <Separator className='my-4' />

                    {(onSave || onDelete) && (
                        <div className="grid grid-cols-1 gap-4 md:flex md:justify-between">
                            {onSave && (
                                <Button type="submit" className="w-full md:w-40">
                                    {submitText ?? "Save"}
                                </Button>
                            )}
                            {onDelete && (
                                <Button
                                    type="button"
                                    variant="destructive"
                                    onClick={onDelete}
                                    className="w-full md:w-40"
                                >
                                    Delete Character
                                </Button>
                            )}
                        </div>
                    )}
                </div>
            )}
        </form>
    );
}
