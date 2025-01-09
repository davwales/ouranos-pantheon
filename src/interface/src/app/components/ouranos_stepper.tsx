import {
    Box,
    Button,
    Step,
    StepLabel,
    Stepper,
    Typography
} from '@mui/material';
import React, { useState } from 'react';

interface OuranosStepperProps {
    steps: {
        label: string;
        component: React.ReactNode;
    }[];
    onComplete?: () => void;
    onStepChange?: (step: number) => void;
};

export default function OuranosStepper(props: OuranosStepperProps) {
    const [activeStep, setActiveStep] = useState(0);

    const handleNext = () => {
        const newStep = activeStep + 1;
        setActiveStep(newStep);

        if (props.onStepChange) {
            props.onStepChange(newStep);
        }

        if (props.onComplete && newStep === props.steps.length) {
            props.onComplete();
        }
    };

    const handleBack = () => {
        const newStep = activeStep - 1;
        setActiveStep(newStep);

        if (props.onStepChange) {
            props.onStepChange(newStep);
        }
    };

    return (
        <Box sx={{ widows: "100%", p: "0.25rem", mx: "auto", background: "none" }}>
            <Stepper activeStep={activeStep} sx={{ mb: "1.5rem" }}>
                {props.steps.map((step, index) => (
                    <Step key={step.label}>
                        <StepLabel>{step.label}</StepLabel>
                    </Step>
                ))}
            </Stepper>

            <Box sx={{ mb: "1rem" }}>
                {activeStep < props.steps.length ? (
                    <Box>
                        {props.steps[activeStep].component}
                    </Box>
                ) : (
                    <Box sx={{ textAlign: "center" }}>
                        <Typography variant="h6" sx={{ mb: 4 }}>All steps completed!</Typography>
                    </Box>
                )}
            </Box>

            <Box sx={{ mx: "auto" }}>
                <Button
                    variant="contained"
                    disabled={activeStep === 0}
                    onClick={handleBack}
                >
                    Back
                </Button>
                <Button
                    variant="contained"
                    sx={{ float: "right" }}
                    onClick={handleNext}
                >
                    {activeStep === props.steps.length - 1 ? 'Finish' : 'Next'}
                </Button>
            </Box>
        </Box>
    );
};
