﻿using System;
using System.Diagnostics;
using System.Linq;

using AlekseyNagovitsyn.BuildVision.Tool.Models;
using AlekseyNagovitsyn.BuildVision.Tool.Models.Settings.BuildMessages;

using EnvDTE;

using ProjectItem = AlekseyNagovitsyn.BuildVision.Tool.Models.ProjectItem;

namespace AlekseyNagovitsyn.BuildVision.Tool.Building
{
    public static class BuildMessages
    {
        public static string GetBuildBeginMajorMessage(
            SolutionItem solutionItem, 
            BuildInfo buildInfo, 
            BuildMessagesSettings labelsSettings)
        {
            var buildAction = buildInfo.BuildAction;
            var buildScope = buildInfo.BuildScope;

            if (buildAction == null || buildScope == null)
                return Resources.UnknownBuildActionOrScope_BuildBeginText;

            if (buildInfo.BuildStartTime == null)
                throw new InvalidOperationException();

            string timeString;
            try
            {
                timeString = buildInfo.BuildStartTime.Value.ToString(labelsSettings.DateTimeFormat);
            }
            catch (FormatException)
            {
                timeString = Resources.InvalidTimeStringFormat;
            }

            string unitName;
            switch (buildScope.Value)
            {
                case vsBuildScope.vsBuildScopeSolution:
                    unitName = Resources.BuildScopeSolution_UnitName;
                    if (labelsSettings.ShowSolutionName)
                        unitName += string.Format(Resources.BuildScopeSolution_SolutionNameTemplate, solutionItem.Name);
                    break;

                case vsBuildScope.vsBuildScopeBatch:
                    unitName = Resources.BuildScopeBatch_UnitName;
                    break;

                case vsBuildScope.vsBuildScopeProject:
                    unitName = Resources.BuildScopeProject_UnitName;
                    if (labelsSettings.ShowProjectName)
                    {
                        var proj = buildInfo.BuildScopeProject;
                        if (proj != null)
                        {
                            unitName += string.Format(Resources.BuildScopeProject_ProjectNameTemplate, proj.Name);
                        }
                        else
                        {
                            unitName = Resources.BuildScopeBatch_UnitName;
                            buildInfo.OverrideBuildProperties(buildScope: vsBuildScope.vsBuildScopeBatch);
                        }
                    }
                    break;

                default:
                    throw new ArgumentOutOfRangeException();
            }

            string actionName;
            string beginAtString;
            switch (buildAction.Value)
            {
                case vsBuildAction.vsBuildActionRebuildAll:
                    actionName = Resources.BuildActionRebuildAll;
                    beginAtString = Resources.BuildActionRebuildAll_BeginAtString;
                    break;

                case vsBuildAction.vsBuildActionBuild:
                    actionName = Resources.BuildActionBuild;
                    beginAtString = Resources.BuildActionBuild_BeginAtString;
                    break;

                case vsBuildAction.vsBuildActionClean:
                    actionName = Resources.BuildActionClean;
                    beginAtString = Resources.BuildActionClean_BeginAtString;
                    break;

                case vsBuildAction.vsBuildActionDeploy:
                    return Resources.UnknownBuildActionOrScope_BuildBeginText;

                default:
                    throw new ArgumentOutOfRangeException();
            }

            string mainString;
            switch (labelsSettings.MajorMessageFormat)
            {
                case BuildMajorMessageFormat.Entire:
                    mainString = string.Format(Resources.BuildBeginStateLabelTemplate_Default, actionName, unitName, beginAtString, timeString);
                    break;

                case BuildMajorMessageFormat.Unnamed:
                    mainString = string.Format(Resources.BuildBeginStateLabelTemplate_ShortForm, actionName, beginAtString, timeString);
                    break;

                default:
                    throw new ArgumentOutOfRangeException();
            }

            string resultMainString = string.Format(labelsSettings.BuildBeginMajorMessageStringFormat, mainString);
            return resultMainString;
        }

        public static string GetBuildBeginExtraMessage(
            BuildInfo buildInfo,
            BuildMessagesSettings labelsSettings)
        {
            if (buildInfo == null
                || buildInfo.BuildStartTime == null
                || !labelsSettings.ShowExtraMessage
                || labelsSettings.ExtraMessageDelay < 0)
            {
                return string.Empty;
            }

            TimeSpan timeSpan = DateTime.Now.Subtract(buildInfo.BuildStartTime.Value);
            if (timeSpan.TotalSeconds > labelsSettings.ExtraMessageDelay)
            {
                string extraTimePartString;
                switch (labelsSettings.ExtraMessageFormat)
                {
                    case BuildExtraMessageFormat.Custom:
                        try
                        {
                            extraTimePartString = timeSpan.ToString(labelsSettings.TimeSpanFormat);
                        }
                        catch (FormatException)
                        {
                            extraTimePartString = Resources.InvalidTimeStringFormat;
                        }
                        break;

                    case BuildExtraMessageFormat.TotalSeconds:
                        extraTimePartString = string.Format("{0}", Math.Truncate(timeSpan.TotalSeconds));
                        break;

                    case BuildExtraMessageFormat.TotalMinutes:
                        extraTimePartString = string.Format("{0}", Math.Truncate(timeSpan.TotalMinutes));
                        break;

                    case BuildExtraMessageFormat.TotalMinutesWithSeconds:
                        extraTimePartString = string.Format("{0:00}:{1:00}", Math.Truncate(timeSpan.TotalMinutes), timeSpan.Seconds);
                        break;

                    default:
                        throw new ArgumentOutOfRangeException();
                }

                string extraString = string.Format(labelsSettings.ExtraMessageStringFormat, extraTimePartString);
                return extraString;
            }

            return string.Empty;
        }

        public static string GetBuildDoneMessage(
            SolutionItem solutionItem,
            BuildInfo buildInfo,
            BuildMessagesSettings labelsSettings)
        {
            return GetBuildDoneMajorMessage(solutionItem, buildInfo, labelsSettings)
                + GetBuildDoneExtraMessage(buildInfo, labelsSettings);
        }

        private static string GetBuildDoneMajorMessage(
            SolutionItem solutionItem,
            BuildInfo buildInfo,
            BuildMessagesSettings labelsSettings)
        {
            if (buildInfo == null)
                return Resources.BuildDoneText_BuildNotStarted;

            var buildAction = buildInfo.BuildAction;
            var buildScope = buildInfo.BuildScope;

            if (buildInfo.BuildFinishTime == null)
                throw new InvalidOperationException();

            string timeString;
            try
            {
                timeString = buildInfo.BuildFinishTime.Value.ToString(labelsSettings.DateTimeFormat);
            }
            catch (FormatException)
            {
                timeString = Resources.InvalidTimeStringFormat;
            }

            if (buildAction == null || buildScope == null)
                return string.Format(Resources.BuildDoneText_NotSupported_BuildActionOrScopeIsNull_CompletedAtTemplate, timeString); //? WTF???

            string unitName;
            switch (buildScope.Value)
            {
                case vsBuildScope.vsBuildScopeSolution:
                    unitName = Resources.BuildScopeSolution_UnitName;
                    if (labelsSettings.ShowSolutionName)
                        unitName += string.Format(Resources.BuildScopeSolution_SolutionNameTemplate, solutionItem.Name);
                    break;

                case vsBuildScope.vsBuildScopeBatch:
                    unitName = Resources.BuildScopeBatch_UnitName;
                    break;

                case vsBuildScope.vsBuildScopeProject:
                    unitName = Resources.BuildScopeProject_UnitName;
                    if (labelsSettings.ShowProjectName)
                    {
                        // Skip dependent projects. The last project in the list is the target project.
                        string uniqProjName = buildInfo.BuildedProjects[buildInfo.BuildedProjects.Count - 1].UniqueName;
                        ProjectItem projItem = solutionItem.Projects.FirstOrDefault(item => item.UniqueName == uniqProjName);
                        Debug.Assert(projItem != null);

                        unitName += string.Format(Resources.BuildScopeProject_ProjectNameTemplate, projItem.Name);
                    }
                    break;

                default:
                    throw new ArgumentOutOfRangeException();
            }

            string actionName;
            string resultName;
            GetBuildDoneActionAndResultStrings(solutionItem, buildInfo, out actionName, out resultName);

            string mainString;
            switch (labelsSettings.MajorMessageFormat)
            {
                case BuildMajorMessageFormat.Entire:
                    mainString = string.Format(Resources.BuildDoneStateLabelTemplate_Default, actionName, unitName, resultName, timeString);
                    break;

                case BuildMajorMessageFormat.Unnamed:
                    mainString = string.Format(Resources.BuildDoneStateLabelTemplate_ShortForm, actionName, resultName, timeString);
                    break;

                default:
                    throw new ArgumentOutOfRangeException();
            }

            string resultMainString = string.Format(labelsSettings.BuildDoneMajorMessageStringFormat, mainString);
            return resultMainString;
        }

        private static void GetBuildDoneActionAndResultStrings(
            SolutionItem solutionItem,
            BuildInfo buildInfo,
            out string actionName,
            out string resultName)
        {
            vsBuildAction? buildAction = buildInfo.BuildAction;

            if (buildAction == null)
                throw new InvalidOperationException();

            switch (buildAction.Value)
            {
                case vsBuildAction.vsBuildActionBuild:
                    actionName = Resources.BuildActionBuild;
                    break;

                case vsBuildAction.vsBuildActionRebuildAll:
                    actionName = Resources.BuildActionRebuildAll;
                    break;

                case vsBuildAction.vsBuildActionClean:
                    actionName = Resources.BuildActionClean;
                    break;

                case vsBuildAction.vsBuildActionDeploy:
                    throw new InvalidOperationException();

                default:
                    throw new ArgumentOutOfRangeException();
            }

            bool buildWithoutErrors = buildInfo.BuildedProjects.BuildWithoutErrors;
            int errorStateProjectsCount = solutionItem.Projects.Count(item => item.State.IsErrorState());

            if (buildInfo.BuildIsCancelled)
                resultName = buildAction.Value == vsBuildAction.vsBuildActionClean ? Resources.BuildActionCancelled_Clean : Resources.BuildActionCancelled;
            else if (!buildWithoutErrors)
                resultName = buildAction.Value == vsBuildAction.vsBuildActionClean ? Resources.BuildActionFailed_Clean : Resources.BuildActionFailed;
            else if (/*buildWithoutErrors &&*/ errorStateProjectsCount == 0)
                resultName = buildAction.Value == vsBuildAction.vsBuildActionClean ? Resources.BuildActionFinishedSuccessfully_Clean : Resources.BuildActionFinishedSuccessfully;
            else
                resultName = buildAction.Value == vsBuildAction.vsBuildActionClean ? Resources.BuildActionFinished_Clean : Resources.BuildActionFinished;
        }

        private static string GetBuildDoneExtraMessage(
            BuildInfo buildInfo, 
            BuildMessagesSettings labelsSettings)
        {
            if (buildInfo == null 
                || buildInfo.BuildStartTime == null
                || buildInfo.BuildFinishTime == null
                || !labelsSettings.ShowExtraMessage)
                return string.Empty;

            TimeSpan timeSpan = buildInfo.BuildFinishTime.Value.Subtract(buildInfo.BuildStartTime.Value);

            string extraTimePartString;
            switch (labelsSettings.ExtraMessageFormat)
            {
                case BuildExtraMessageFormat.Custom:
                    try
                    {
                        extraTimePartString = timeSpan.ToString(labelsSettings.TimeSpanFormat);
                    }
                    catch (FormatException)
                    {
                        extraTimePartString = Resources.InvalidTimeStringFormat;
                    }
                    break;

                case BuildExtraMessageFormat.TotalSeconds:
                    extraTimePartString = string.Format("{0}", Math.Truncate(timeSpan.TotalSeconds));
                    break;

                case BuildExtraMessageFormat.TotalMinutes:
                    extraTimePartString = string.Format("{0}", Math.Truncate(timeSpan.TotalMinutes));
                    break;

                case BuildExtraMessageFormat.TotalMinutesWithSeconds:
                    extraTimePartString = string.Format("{0:00}:{1:00}", Math.Truncate(timeSpan.TotalMinutes), timeSpan.Seconds);
                    break;

                default:
                    throw new ArgumentOutOfRangeException();
            }

            string extraString = string.Format(labelsSettings.ExtraMessageStringFormat, extraTimePartString);
            return extraString;
        }
    }
}