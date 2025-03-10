import Typography from '@/app/components/core/data-display/typography';
import Button from '@/app/components/core/inputs/button';
import Box from '@/app/components/core/layout/box';
import { StepDefinition, Stepper } from '@/app/components/core/navigation/stepper';
import { StyleProps } from '@/app/components/core/style_props';
import { useState } from 'react';

interface DetailedStepperProps {
    styling?: StyleProps;
    steps: StepDefinition[];
    onComplete?: () => void;
    onStepChange?: (step: number) => void;
};

export default function DetailedStepper(props: DetailedStepperProps) {
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
        <Box styling={props.styling}>
            <Stepper steps={props.steps} activeStep={activeStep} styling={{ mb: 'medium' }} />

            <Box styling={{ mb: "small" }}>
                {activeStep < props.steps.length ? (
                    <Box>
                        {props.steps[activeStep].component}
                    </Box>
                ) : (
                    <Box styling={{ textAlign: "center" }}>
                        <Typography variant="h6" styling={{ mb: 'large' }}>All steps completed!</Typography>
                    </Box>
                )}
            </Box>

            <Box styling={{ mx: "auto" }}>
                <Button
                    variant='outlined'
                    color='secondary'
                    disabled={activeStep === 0}
                    onClick={handleBack}
                >
                    Back
                </Button>
                <Button
                    variant="contained"
                    styling={{ float: "right" }}
                    onClick={handleNext}
                >
                    {activeStep === props.steps.length - 1 ? 'Finish' : 'Next'}
                </Button>
            </Box>
        </Box>
    );
};
