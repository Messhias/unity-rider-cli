package com.jetbrains.rider.plugins.unityclipipeline.settings

import com.intellij.openapi.options.BoundConfigurable
import com.intellij.openapi.ui.DialogPanel
import com.intellij.ui.dsl.builder.bindIntText
import com.intellij.ui.dsl.builder.bindText
import com.intellij.ui.dsl.builder.panel

class UnityCliPipelineConfigurable : BoundConfigurable("Unity CLI Test Runner") {
    private val settings = com.intellij.openapi.components.service<UnityCliPipelineSettings>()

    override fun createPanel(): DialogPanel = panel {
        row("Unity CLI path:") {
            textField()
                .bindText(settings::cliPath)
                .comment("Leave empty to use PATH / UNITY_CLI_PATH.")
                .resizableColumn()
        }
        row("Default test timeout (seconds):") {
            intTextField(range = 1..86_400)
                .bindIntText(settings::defaultTimeoutSeconds)
                .comment("Mapped from UNITY_TEST_TIMEOUT when unset in the UI.")
        }
    }
}
