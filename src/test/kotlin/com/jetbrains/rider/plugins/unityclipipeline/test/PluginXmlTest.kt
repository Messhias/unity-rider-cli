package com.jetbrains.rider.plugins.unityclipipeline.test

import org.testng.Assert.assertEquals
import org.testng.Assert.assertTrue
import org.testng.annotations.Test
import java.io.File
import javax.xml.parsers.DocumentBuilderFactory

class PluginXmlTest {

    private val pluginXmlFile: File
        get() = File("src/rider/main/resources/META-INF/plugin.xml").absoluteFile

    @Test
    fun `plugin xml is well-formed and aligned with product identity`() {
        assertTrue(pluginXmlFile.isFile, "Expected plugin.xml at ${pluginXmlFile.path}")

        val document = DocumentBuilderFactory.newInstance()
            .newDocumentBuilder()
            .parse(pluginXmlFile)
        document.documentElement.normalize()

        assertEquals(document.documentElement.tagName, "idea-plugin")

        val id = document.getElementsByTagName("id").item(0).textContent.trim()
        val name = document.getElementsByTagName("name").item(0).textContent.trim()
        val description = document.getElementsByTagName("description").item(0).textContent

        assertEquals(id, "com.jetbrains.rider.plugins.unityclipipeline")
        assertEquals(name, "Unity CLI Test Runner")
        assertTrue(
            description.contains("Unity Test Framework", ignoreCase = true),
            "Description should mention Unity Test Framework"
        )
        assertTrue(
            description.contains("unity test", ignoreCase = true),
            "Description should mention unity test CLI"
        )
        assertTrue(
            description.contains("Coexistence", ignoreCase = true),
            "Description should document coexistence with Unity Support"
        )

        val depends = document.getElementsByTagName("depends")
        val dependencyIds = (0 until depends.length).map { depends.item(it).textContent.trim() }
        assertTrue(
            dependencyIds.contains("com.intellij.modules.rider"),
            "Must depend on Rider module"
        )
        assertTrue(
            !dependencyIds.contains("com.intellij.resharper.unity"),
            "Must not hard-depend on Unity Support (coexistence without required install)"
        )
    }
}
