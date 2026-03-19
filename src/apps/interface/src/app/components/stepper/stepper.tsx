"use client";

import { Step } from "@/app/components/stepper/types";
import { Typography } from "@/app/components/typography";
import { Button } from "@/components/ui/button";
import { Collapsible, CollapsibleContent, CollapsibleTrigger } from "@/components/ui/collapsible";
import { ChevronsUpDown } from "lucide-react";
import React, { useState } from "react";

export default function Stepper({
    steps,
    onComplete,
    ...props
}: React.ComponentProps<"div"> & {
    steps: Step[];
    onComplete: () => void;
}) {
    const [currentStep, setCurrentStep] = useState<number>(0);
    const [stepsOpen, setStepsOpen] = useState<boolean[]>(steps.map((_, index) => index === 0));

    const handleCollapseToggle = (index: number, open: boolean) => {
        const updatedStepsOpen = [...stepsOpen];
        updatedStepsOpen[index] = open;
        setStepsOpen(updatedStepsOpen);
    };

    const handlePrevious = () => {
        const clampedStep = Math.max(0, currentStep - 1);
        setStepsOpen(stepsOpen.map((_, index) => index == clampedStep));
        setCurrentStep(clampedStep);
    };

    const handleNext = () => {
        if (currentStep + 1 >= steps.length) {
            onComplete();
            return;
        }

        const desiredStep = currentStep + 1;
        setStepsOpen(stepsOpen.map((_, index) => index == desiredStep));
        setCurrentStep(desiredStep);
    };

    return (
        <div {...props}>
            <div>
                {steps.map((step, stepIndex) => (
                    <Collapsible
                        key={stepIndex}
                        open={stepsOpen[stepIndex]}
                        onOpenChange={(open) => handleCollapseToggle(stepIndex, open)}
                        className="my-4"
                    >
                        <CollapsibleTrigger className="hover:cursor-pointer border-b-1 w-full flex justify-between items-center">
                            <Typography variant="h2" className="border-b-0">{step.label}</Typography>
                            <ChevronsUpDown />
                        </CollapsibleTrigger>
                        <CollapsibleContent>
                            {step.content}
                            {currentStep == stepIndex && (
                                <div className="grid grid-cols-1 gap-4 md:flex md:justify-between mt-4">
                                    <Button
                                        variant="secondary"
                                        disabled={currentStep == 0}
                                        onClick={handlePrevious}
                                        className="w-full md:w-40"
                                    >
                                        Previous
                                    </Button>

                                    <Button
                                        onClick={handleNext}
                                        className="w-full md:w-40"
                                    >
                                        {currentStep == steps.length - 1 ? "Complete" : "Next"}
                                    </Button>
                                </div>
                            )}
                        </CollapsibleContent>
                    </Collapsible>
                ))}
            </div>
        </div>
    );
}